#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include "debug.h"
#include "common.h"
#include "machine_config.h"
#include "utility.h"
#include "string_hash.h"
#include "int_hash.h"
#include "llist.h"
#include "reflection.h"
#include "parse.h"
#include "naudio.h"
#include "kvp.h"

/**
 * This file implements a finite state machine to parse a text .coef file.
 * This is loaded into a new ALADPCMBook. There is a .graphml (and .svg)
 * listing valid state transitions.
 * 
 * The properties can be read in any order.
*/

/**
 * Max length in bytes to accept as token length.
*/
#define IDENTIFIER_MAX_LEN 20

/**
 * Local singleton containing the parser context.
*/
struct CoefParseContext {

    /**
     * Most recently resolved or current property desriptor.
    */
    struct RuntimeTypeInfo *current_property;

    /**
     * Current line from input file.
    */
    int current_line;

    /**
     * Text buffer to store property type name.
    */
    char *property_name_buffer;

    /**
     * Current position in `property_name_buffer`.
    */
    int property_name_buffer_pos;

    /**
     * Number of csv items read so far.
    */
    int list_count;

    /**
     * Text buffer to store property value as it's being read.
    */
    char *property_value_buffer;

    /**
     * Current position in `property_value_buffer`.
    */
    int property_value_buffer_pos;

    /**
     * After property value is finished being read,
     * if it's an integer it will be converted
     * and stored in this property.
    */
    int current_value_int;

    /**
     * List of code book values read.
    */
    struct LinkedList *book_val;

    struct ALADPCMBook *book;
};

/**
 * Properties supported under `bank`.
*/
enum CoefBookPropertyId {
    /**
     * Default / unset / unknown.
    */
    COEF_BOOK_PROPERTY_DEFAULT_UNKNOWN = 0,

    /**
     * .coef name: order
     * class: struct ALADPCMBook->order
    */
    COEF_BOOK_PROPERTY_ORDER = 1,

    /**
     * .coef name: npredictors
     * class: struct ALADPCMBook->npredictors
    */
    COEF_BOOK_PROPERTY_NPREDICTORS,

    /**
     * .coef name: book
     * class: struct ALADPCMBook->book
    */
    COEF_BOOK_PROPERTY_BOOK
};

/**
 * Describes properties of ALADPCMBook.
*/
static struct TypeInfo CoefBookProperties[] = {
    { COEF_BOOK_PROPERTY_ORDER, "order", TYPE_ID_INT },
    { COEF_BOOK_PROPERTY_NPREDICTORS, "npredictors", TYPE_ID_INT },
    { COEF_BOOK_PROPERTY_BOOK, "book", TYPE_ID_CSV_INT_LIST }
};
static const int CoefBookProperties_len = ARRAY_LENGTH(CoefBookProperties);

/**
 * Parse states used by finite state machine.
 * See graphml or svg for valid transitions.
*/
enum CoefParseState {
    /**
     * Begin state.
    */
    COEF_PARSE_STATE_INITIAL_INSTANCE_PROPERTY = 1,

    /**
     * Reading the property name.
    */
    COEF_PARSE_STATE_PROPERTY_NAME,

    /**
     * The property requires an equal sign,
     * so search for that.
    */
    COEF_PARSE_STATE_EQUAL_SIGN,

    /**
     * The property requires an integer value,
     * search for text of that kind.
    */
    COEF_PARSE_STATE_INITIAL_INT_VALUE,

    /**
     * Reading the property value as an integer.
    */
    COEF_PARSE_STATE_INT_VALUE,

    /**
     * Done reading property assignment,
     * search for ';'.
    */
    COEF_PARSE_STATE_SEARCH_SEMI_COLON
};

// forward declarations

static struct CoefParseContext *CoefParseContext_new(void);
static void CoefParseContext_free(struct CoefParseContext *context);
static void buffer_append_inc(char *buffer, int *position, char c);
static void get_property(const char *property_name, struct RuntimeTypeInfo *property);
static void set_current_property_value_int(struct CoefParseContext *context);
static void apply_property_on_book(struct CoefParseContext *context);
static void check_resolve(struct CoefParseContext *context);

// end forward declarations

/**
 * Allocates memory for a new context.
 * @returns: pointer to new context.
*/
static struct CoefParseContext *CoefParseContext_new()
{
    TRACE_ENTER(__func__)

    struct CoefParseContext *context = (struct CoefParseContext*)malloc_zero(1, sizeof(struct CoefParseContext));

    context->property_name_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    context->property_value_buffer = (char *)malloc_zero(1, IDENTIFIER_MAX_LEN);
    
    context->current_property = (struct RuntimeTypeInfo *)malloc_zero(1, sizeof(struct RuntimeTypeInfo));
    
    context->book_val = LinkedList_new();

    context->book = (struct ALADPCMBook *)malloc_zero(1, sizeof(struct ALADPCMBook));

    TRACE_LEAVE(__func__)

    return context;
}

/**
 * Frees all memory associated with context.
 * @param context: object to free.
*/
static void CoefParseContext_free(struct CoefParseContext *context)
{
    TRACE_ENTER(__func__)

    free(context->current_property);

    free(context->property_value_buffer);
    free(context->property_name_buffer);

    LinkedList_free(context->book_val);
    
    free(context);

    TRACE_LEAVE(__func__)
}

/**
 * Appends character to buffer and increments position.
 * No length / overflow checks are performed.
 * @param buffer: buffer to append to.
 * @param position: out parameter. Current position to place character. Will be incremented.
 * @param c: character to add to bufer.
*/
static void buffer_append_inc(char *buffer, int *position, char c)
{
    buffer[*position] = c;
    *position = *position + 1;
}

/**
 * For a given type, resolves text to a property info.
 * No memory is allocated.
 * @param property_name: text to get property from.
 * @param property: out paramater. Will have properties set explaining type.
*/
static void get_property(const char *property_name, struct RuntimeTypeInfo *property)
{
    TRACE_ENTER(__func__)

    int i;

    for (i=0; i<CoefBookProperties_len; i++)
    {
        if (strcmp(property_name, CoefBookProperties[i].value) == 0)
        {
            property->key = CoefBookProperties[i].key;
            property->type_id = CoefBookProperties[i].type_id;

            TRACE_LEAVE(__func__)
            return;
        }
    }

    stderr_exit(EXIT_CODE_GENERAL, "%s %d>: cannot resolve property_name \"%s\"\n", __func__, __LINE__, property_name);

    TRACE_LEAVE(__func__)
}

/**
 * Parses {@code context->property_value_buffer} as integer and sets
 * {@code context->current_value_int} to the parsed value. If array
 * is empty (position is zero), value is set to zero. The original
 * buffer remains unchanged (this can be called multiple times).
 * @param context: context.
*/
static void set_current_property_value_int(struct CoefParseContext *context)
{
    TRACE_ENTER(__func__)

    int val;
    char *pend = NULL;

    if (context->property_value_buffer_pos == 0)
    {
        context->current_value_int = 0;

        TRACE_LEAVE(__func__)
        return;
    }

    val = strtol(context->property_value_buffer, &pend, 0);
                
    if (pend != NULL && *pend == '\0')
    {
        if (errno == ERANGE)
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> error (range), cannot parse context->property_value_buffer as integer: %s\n", __func__, __LINE__, context->property_value_buffer);
        }

        context->current_value_int = val;
    }
    else
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d> error, cannot parse context->property_value_buffer as integer: %s\n", __func__, __LINE__, context->property_value_buffer);
    }

    TRACE_LEAVE(__func__)
}

/**
 * This resolves the current property info and sets the value from
 * {@code context->property_value_buffer}.
 * 
 * @param context: context.
*/
static void apply_property_on_book(struct CoefParseContext *context)
{
    TRACE_ENTER(__func__)

    if (context == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context is NULL\n", __func__, __LINE__);
    }

    if (context->book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->book is NULL\n", __func__, __LINE__);
    }

    struct ALADPCMBook *book = context->book;

    switch (context->current_property->key)
    {
        case COEF_BOOK_PROPERTY_ORDER:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set book order=%d\n", context->current_value_int);
            }
            
            book->order = (int32_t)context->current_value_int;
        }
        break;

        case COEF_BOOK_PROPERTY_NPREDICTORS:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("set book npredictors=%d\n", context->current_value_int);
            }
            
            book->npredictors = (int32_t)context->current_value_int;
        }
        break;

        case COEF_BOOK_PROPERTY_BOOK:
        {
            set_current_property_value_int(context);

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("append codebook entry 0x%08x\n", context->current_value_int);
            }

            if (context->current_value_int > UINT16_MAX)
            {
                fflush_printf(stderr, "context->current_value_int=%d exceeds max=%d, clamping\n", context->current_value_int, UINT16_MAX);
                context->current_value_int = UINT16_MAX;
            }

            if (context->current_value_int < INT16_MIN)
            {
                fflush_printf(stderr, "context->current_value_int=%d under mind=%d, clamping\n", context->current_value_int, INT16_MIN);
                context->current_value_int = INT16_MIN;
            }
            
            struct LinkedListNode *node = LinkedListNode_new();

            // codebook values are stored big endian
            node->data_local = BSWAP16_INLINE((uint16_t)context->current_value_int);

            LinkedList_append_node(context->book_val, node);

            context->list_count++;
        }
        break;

        default:
        {
            stderr_exit(EXIT_CODE_GENERAL, "%s %d>: context->current_property->key not supported: %d\n", __func__, __LINE__, context->current_property->key);
        }
        break;
    }

    context->property_value_buffer_pos = 0;
    context->current_value_int = 0;
    memset(context->property_value_buffer, 0, IDENTIFIER_MAX_LEN);

    TRACE_LEAVE(__func__)
}

static void check_resolve(struct CoefParseContext *context)
{
    TRACE_ENTER(__func__)

    if (context == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context is NULL\n", __func__, __LINE__);
    }

    if (context->book == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: context->book is NULL\n", __func__, __LINE__);
    }

    struct ALADPCMBook *book = context->book;
    struct LinkedListNode *node;
    int i;

    if (book->order == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: book->order not set or zero\n", __func__, __LINE__);
    }

    if (book->npredictors == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: book->npredictors not set or zero\n", __func__, __LINE__);
    }

    if (context->list_count == 0)
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: no codebook values were read!\n", __func__, __LINE__);
    }

    if (context->book_val->count != (size_t)(book->order * book->npredictors * 8))
    {
        stderr_exit(EXIT_CODE_GENERAL, "%s %d>: expected %d codebook values, read %d\n", __func__, __LINE__, (book->order * book->npredictors * 8), context->book_val->count);
    }

    size_t book_bytes = book->order * book->npredictors * 16;
    book->book = (int16_t *)malloc_zero(1, book_bytes);

    for (
        i = 0, node = context->book_val->head;
        (size_t)i < context->book_val->count && node != NULL;
        i++, node = node->next)
    {
        book->book[i] = (uint16_t)node->data_local;
        node->data_local = 0;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Reads a .coef file and parses into a {@code struct ALADPCMBook}.
 * This allocates memory.
 * This is the public parse entry point.
 * @param fi: file info object of file to parse.
 * @returns: new book parsed from .coef file.
*/
struct ALADPCMBook *ALADPCMBook_new_from_coef(struct FileInfo *fi)
{
    TRACE_ENTER(__func__)

    /**
     * Debug helper, contains text of current line.
    */
    char line_buffer[MAX_FILENAME_LEN];

    /**
     * Debug helper, position in line_buffer.
    */
    int line_buffer_pos;

    /**
     * Current character read or being processed.
    */
    char c = 0;

    /**
     * Character `c` as integer. Promoted to int to status as valid
     * or invalid (equal to int32_t -1).
    */
    int c_int;

    /**
     * Second most recent charactere read or being processed.
     * Promoted to int to status as valid
     * or invalid (equal to int32_t -1).
    */
    int previous_c;

    /**
     * Previous state of parsing finite state machine.
     * This is mostly used after finishing reading a comment
     * to know which state to return to.
    */
    int previous_state;

    /**
     * Current state of parsing finite state machine.
    */
    int state;

    /**
     * Current line number as read from .inst file.
    */
    int current_line_number;

    /**
     * Current position of .inst file in bytes.
    */
    size_t pos;

    /**
     * Total length of .inst file in bytes.
    */
    size_t len;

    /**
     * The entire .inst file is read into memory and stored
     * in this buffer.
    */
    char *file_contents;

    /**
     * Flag used in finite state machine to indicate the current state
     * should process the current context in some way.
    */
    int terminate;

    /**
     * Sometimes the current state ends but needs to understand context or delay processing
     * until the next state. For example, applying the property should happen on reading
     * a semi colon, but reading-the-value-state can abruptly end when a semi colon is read.
     * In that case, transition to the new state, but mark the character as needing to be
     * processed again.
    */
    int replay;

    // begin initial setup.

    struct CoefParseContext *context = CoefParseContext_new();

    memset(line_buffer, 0, MAX_FILENAME_LEN);

    // read .coef file into memory
    FileInfo_fseek(fi, 0, SEEK_SET);
    file_contents = (char *)malloc_zero(1, fi->len);
    len = FileInfo_fread(fi, file_contents, fi->len, 1);

    c_int = -1;
    previous_c = -1;
    current_line_number = 1;
    pos = 0;
    line_buffer_pos = 0;
    state = COEF_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;
    len = fi->len;
    replay = 0;

    // done with initial setup.

    /**
     * This is the parsing finite state machine.
     * Iterate until reaching end of file.
    */
    while (pos < len)
    {
        if (replay)
        {
            replay = 0;

            if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
            {
                printf("state=%d, source line=%d pos=%ld> replay previous character\n", state, current_line_number, pos);
            }
        }
        else
        {
            previous_c = c_int;

            if (pos < len)
            {
                c = file_contents[pos];
            }

            if ((line_buffer_pos + 1) > MAX_FILENAME_LEN)
            {
                stderr_exit(EXIT_CODE_GENERAL, "%s %d> buffer overflow (line position %d) readline line, pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, line_buffer_pos, pos, current_line_number, state);
            }

            line_buffer[line_buffer_pos] = c;
            c_int = 0xff & (int)c;

            pos++;
            line_buffer_pos++;

            // any '\n' or '\r' increments the count.
            if (is_newline(c))
            {
                current_line_number++;
                memset(line_buffer, 0, MAX_FILENAME_LEN);
                line_buffer_pos = 0;
            }
            
            // but if this is windows, adjust for "\r\n" double counting.
            if (is_windows_newline(c_int, previous_c))
            {
                current_line_number--;
            }

            context->current_line = current_line_number;
        }

        /**
         * See the graphml or svg for state transitions.
        */
        switch (state)
        {
            case COEF_PARSE_STATE_INITIAL_INSTANCE_PROPERTY:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_alpha(c))
                {
                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, c);
                    previous_state = state;
                    state = COEF_PARSE_STATE_PROPERTY_NAME;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case COEF_PARSE_STATE_PROPERTY_NAME:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    terminate = 1;
                }
                else if (is_alphanumeric(c))
                {
                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, c);
                }
                else if (c == '=')
                {
                    terminate = 1;
                    replay = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    /**
                     * This will be the next state to transition to.
                     * The challenge is that this is context dependent and can't be decided
                     * until after the current property type is known (which is what is
                     * currently being parsed).
                     * 
                     * Note that the `replay` flag simplifies the number of states to consider,
                     * otherwise (e.g.), would have to skip '=' and jump to state after that, etc.
                    */
                    int terminate_state;

                    buffer_append_inc(context->property_name_buffer, &context->property_name_buffer_pos, '\0');
                    get_property(context->property_name_buffer, context->current_property);

                    if (context->current_property->key == 0)
                    {
                        stderr_exit(EXIT_CODE_GENERAL, "%s %d> cannot determine property for \"%s\", pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, context->property_name_buffer, pos, current_line_number, state);
                    }

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate property name=\"%s\"\n", state, current_line_number, pos, context->property_name_buffer);
                    }

                    context->property_name_buffer_pos = 0;
                    memset(context->property_name_buffer, 0, IDENTIFIER_MAX_LEN);

                    // now that it's known what needs to happen next, set the next state.
                    if (context->current_property->type_id == TYPE_ID_INT)
                    {
                        terminate_state = COEF_PARSE_STATE_EQUAL_SIGN;
                    }
                    // same as regular int
                    else if (context->current_property->type_id == TYPE_ID_CSV_INT_LIST)
                    {
                        terminate_state = COEF_PARSE_STATE_EQUAL_SIGN;
                    }
                    else
                    {
                        stderr_exit(
                            EXIT_CODE_GENERAL,
                            "%s %d> cannot resolve next state for property, property key=%d, property type_id=%d, pos=%ld, source line=%d, state=%d, previous_state=%d\n",
                            __func__,
                            __LINE__,
                            context->current_property->key,
                            context->current_property->type_id,
                            pos,
                            current_line_number,
                            state,
                            previous_state);
                    }

                    previous_state = state;
                    state = terminate_state;
                }
            }
            break;

            case COEF_PARSE_STATE_EQUAL_SIGN:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == '=')
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_INITIAL_INT_VALUE;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case COEF_PARSE_STATE_INITIAL_INT_VALUE:
            {
                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (is_numeric_int(c))
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_INT_VALUE;

                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }
            }
            break;

            case COEF_PARSE_STATE_INT_VALUE:
            {
                int csv_apply_now = 0;
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                }
                else if (is_numeric_int(c))
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, c);
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_SEARCH_SEMI_COLON;

                    terminate = 1;
                    replay = 1;
                }
                // comma can only show up in a list.
                else if (c == ',' && context->current_property->type_id == TYPE_ID_CSV_INT_LIST)
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_INITIAL_INT_VALUE;

                    terminate = 1;
                    replay = 0; // no replay
                    csv_apply_now = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    buffer_append_inc(context->property_value_buffer, &context->property_value_buffer_pos, '\0');

                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate int value=\"%s\"\n", state, current_line_number, pos, context->property_value_buffer);
                    }

                    if (csv_apply_now)
                    {
                        apply_property_on_book(context);
                    }
                }
            }
            break;

            case COEF_PARSE_STATE_SEARCH_SEMI_COLON:
            {
                terminate = 0;

                if (is_whitespace(c) || is_newline(c))
                {
                    // nothing to do
                }
                else if (c == ';')
                {
                    previous_state = state;
                    state = COEF_PARSE_STATE_INITIAL_INSTANCE_PROPERTY;

                    terminate = 1;
                }
                else
                {
                    stderr_exit(EXIT_CODE_GENERAL, "%s %d> unexpected character '%c', pos=%ld, source line=%d, state=%d\n", __func__, __LINE__, c, pos, current_line_number, state);
                }

                if (terminate)
                {
                    if (DEBUG_PARSE_INST && g_verbosity >= VERBOSE_DEBUG)
                    {
                        printf("state=%d, source line=%d pos=%ld> terminate (;)\n", state, current_line_number, pos);
                    }

                    // no need for special cases for csv here, since only transition
                    // to INITIAL_INT was allowed.
                    apply_property_on_book(context);
                }
            }
            break;

            default:
            stderr_exit(EXIT_CODE_GENERAL, "%s %d> Invalid parse state: %d\n", __func__, __LINE__, state);
        }
    }

    /**
     * The state machine will exit before processing a final entry that ends
     * with end-of-file, so process that now.
    */
    if (c == ';' && context->property_value_buffer_pos > 0)
    {
        apply_property_on_book(context);
    }

    if (g_verbosity >= VERBOSE_DEBUG)
    {
        printf("exit parse state machine\n");
    }

    // Done with reading file, can release memory.
    free(file_contents);

    // last check, and put the codebook array into the book.
    check_resolve(context);

    // save reference because context is about to get freed.
    struct ALADPCMBook *result = context->book;

    // free all memory except the book.
    CoefParseContext_free(context);

    TRACE_LEAVE(__func__)

    return result;
}