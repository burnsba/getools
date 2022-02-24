#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <x86intrin.h>
#include <stdlib.h>
#include "machine_config.h"
#include "debug.h"
#include "utility.h"
#include "md5.h"

/**
 * Easy MD5, implemented per https://en.wikipedia.org/wiki/MD5
 * Unoptimized.
*/

int32_t md5_shift[64] = {
    7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,  7, 12, 17, 22,
    5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,  5,  9, 14, 20,
    4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,  4, 11, 16, 23,
    6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21,  6, 10, 15, 21 };
 
int32_t md5_K[64] = {
    0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
    0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
    0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
    0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
    0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
    0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
    0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
    0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
    0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
    0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
    0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
    0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
    0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
    0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
    0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
    0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391 };

int ascii_to_int(char c)
{
    if (c >= '0' && c <= '9')
    {
        c -= '0';
    }
    else if (c >= 'a' && c <= 'z')
    {
        c = 10 + c - 'a';
    }
    else if (c >= 'A' && c <= 'Z')
    {
        c = 10 + c - 'A';
    }

    return c;
}

/**
 * Compares byte array to array of ASCII characters.
 * @param expected: uint8_t byte values
 * @param actual: ASCII text string.
 * @returns: zero on match, one otherwise (different).
*/
int md5_compare(char *expected, char *actual)
{
    int i;
    for (i=0; i<16; i++)
    {
        int ch;

        ch = 0;
        ch |= ascii_to_int(actual[i*2]) << 4;
        ch |= ascii_to_int(actual[i*2+1]);

        if ((uint8_t)expected[i] != (uint8_t)ch)
        {
            return 1;
        }
    }

    return 0;
}

/**
 * Generates MD5 on input string.
 * No memory allocations are performed.
 * @param str: string to hash, or byte array.
 * @param str_len: length of input string or byte array. Should not include terminating zero if string.
 * @param digest: out parameter. Will contain 128 bit hash result. Must be previously allocated.
*/
void md5_hash(char *str, size_t str_len, char *digest)
{
    TRACE_ENTER(__func__)

    if (str == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: str is NULL\n", __func__);
    }
   
    if (digest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: digest is NULL\n", __func__);
    }
   
    // Initialize variables:
    int32_t a0 = 0x67452301; // A
    int32_t b0 = 0xefcdab89; // B
    int32_t c0 = 0x98badcfe; // C
    int32_t d0 = 0x10325476; // D

    int32_t message_buffer[16];

    // length of input string, without terminating zero, in bits.
    int32_t original_length_bits = str_len * 8;

    // Input will be processed in chunks of up to 64 bytes. This is the number of chunks.
    int iteration_count = str_len / 64;
   
    // The spec calls for appending a single `1` bit then padding with zeroes until
    // reaching 56 mod 64. If the input chunk ends between 56 and 64 bytes then
    // one additional chunk will need to be processed.
    int last_iteration_bytes = str_len % 64;
    if (last_iteration_bytes > 55)
    {
        iteration_count++;
    }

    int iter;
    int i;
    int pos = 0;
    int tail_done = 0;

    for (iter=0; iter<=iteration_count; iter++)
    {
        // Number of input bytes to process in this chunk.
        int iteration_byte_len = (int)str_len - pos;
       
        // Max allowed input bytes per chunk is 64.
        if (iteration_byte_len > 64)
        {
            iteration_byte_len = 64;
        }
       
        // Copy input bytes to buffer.
        memset(message_buffer, 0, 64);
        memcpy(message_buffer, &str[pos], iteration_byte_len);
        pos += iteration_byte_len;

        // The `1` bit that needs to be appended can fall in the last section
        // between 56 and 64 bytes. If that's the case, don't append again
        // on the final iteration.
        if (iteration_byte_len < 64 && !tail_done)
        {
            tail_done = 1;
            ((unsigned char*)message_buffer)[iteration_byte_len] = 0x80;
        }
       
        // Any iteration with fewer than 56 bytes will be the last, so
        // append the input length in that case.
        if (iteration_byte_len < 56)
        {
            memcpy(&((unsigned char*)message_buffer)[56], &original_length_bits, 4);
        }
       
        int32_t A = a0;
        int32_t B = b0;
        int32_t C = c0;
        int32_t D = d0;
       
        int32_t F, g;
       
        for (i=0; i<64; i++)
        {
            if (i <= 15)
            {
                F = (B & C) | ((~B) & D);
                g = i;
            }
            else if (i > 15 && i <= 31)
            {
                F = (D & B) | ((~D) & C);
                g = (5*i + 1) % 16;
            }
            else if (i > 31 && i <= 47)
            {
                F = B ^ C ^ D;
                g = (3*i + 5) % 16;
            }
            else if (i > 47 && i < 64)
            {
                F = C ^ (B | (~D));
                g = (7*i) % 16;
            }
           
            // ignore overflow
            F = F + A + md5_K[i] + message_buffer[g];
            A = D;
            D = C;
            C = B;
           
            // x86 intrinsic: `_rotl`
            // Otherwise, requires rotate left implementation
            B = B + _rotl(F, md5_shift[i]);
        }
       
        // ignore overflow
        a0 = a0 + A;
        b0 = b0 + B;
        c0 = c0 + C;
        d0 = d0 + D;
    }
   
    memcpy(&digest[0], &a0, 4);
    memcpy(&digest[4], &b0, 4);
    memcpy(&digest[8], &c0, 4);
    memcpy(&digest[12], &d0, 4);

    TRACE_LEAVE(__func__)
}