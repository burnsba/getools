#include "parse.h"

/**
 * Checks whether a character is whitespace or not.
 * @param c: character.
 * @returns: true if '\t' or ' ', false otherwise.
*/
int is_whitespace(char c)
{
    return c == ' ' || c == '\t';
}

/**
 * Checks whether a character is a "newline" or not.
 * @param c: character.
 * @returns: true if '\r' or '\n', false otherwise.
*/
int is_newline(char c)
{
    return c == '\r' || c == '\n';
}

/**
 * Checks whether a two character sequence is a windows neweline glyph.
 * @param c: most recent character.
 * @param previous_c: character before the most recent.
 * @returns: true if '\r\n', false otherwise.
*/
int is_windows_newline(int c, int previous_c)
{
    if (c == '\n' && previous_c == '\r')
    {
        return 1;
    }

    return 0;
}

/**
 * Checks whether a character is a valid leading token name character.
 * @param c: character.
 * @returns: true if (regex: [a-zA-z_] ), false otherwise.
*/
int is_alpha(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
}

/**
 * Checks whether a character is a valid token name character.
 * @param c: character.
 * @returns: true if (regex: [a-zA-z0-9_] ), false otherwise.
*/
int is_alphanumeric(char c)
{
    return  (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
}

/**
 * Checks whether a character is a number or not. This is more
 * restrictive than {@code is_numeric_int}.
 * @param c: character.
 * @returns: true if (regex: [0-9] ), false otherwise.
*/
int is_numeric(char c)
{
    return  (c >= '0' && c <= '9');
}

/**
 * Checks whether a character could be used to describe an integer;
 * this allows hex representation.
 * @param c: character.
 * @returns: true if (regex: [0-9xXa-fA-F-] ), false otherwise.
*/
int is_numeric_int(char c)
{
    return  (c >= '0' && c <= '9') || c == 'x' || c == 'X' || c == '-' || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
}

/**
 * Checks whether a character begins a comment.
 * @param c: character.
 * @returns: true if '#', false otherwise.
*/
int is_comment(char c)
{
    return c == '#';
}