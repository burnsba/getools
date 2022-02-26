#include <stdio.h>
#include <string.h>
#include "debug.h"
#include "machine_config.h"
#include "utility.h"
#include "llist.h"
#include "kvp.h"

/**
 * This file contains methods for a simple doubly linked list.
 * Support for a simple text node is included.
*/

static int32_t next_llist_node_id = 0;

/**
 * Appends a node to the list and increments list count.
 * @param root: list to add node to.
 * @param node: node to add to list.
*/
void llist_root_append_node(struct llist_root *root, struct llist_node *node)
{
    TRACE_ENTER(__func__)

    root->count++;

    if (root->root == NULL)
    {
        root->root = node;
        root->tail = node;
    }
    else
    {
        if (root->tail == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: tail is NULL\n", __func__);
        }

        node->prev = root->tail;
        root->tail->next = node;
        root->tail = node;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Allocates memory for a new node.
 * @returns: pointer to new node.
*/
struct llist_node *llist_node_new()
{
    TRACE_ENTER(__func__)

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));

    node->id = next_llist_node_id;
    next_llist_node_id++;

    TRACE_LEAVE(__func__)

    return node;
}

/**
 * Allocates memory for a new node, and copies values from source to new node.
 * Pointer of data is copied, beware duplicate references.
 * @returns: pointer to new node.
*/
struct llist_node *llist_node_copy(struct llist_node *source)
{
    TRACE_ENTER(__func__)

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));

    node->id = next_llist_node_id;
    next_llist_node_id++;

    node->data = source->data;
    node->data_local = source->data_local;

    TRACE_LEAVE(__func__)

    return node;
}

/**
 * Allocates memory for a new root.
 * @returns: pointer to new root.
*/
struct llist_root *llist_root_new()
{
    TRACE_ENTER(__func__)

    struct llist_root *root = (struct llist_root *)malloc_zero(1, sizeof(struct llist_root));

    TRACE_LEAVE(__func__)

    return root;
}

/**
 * Allocates memory for a new node, and for a {@code struct string_data} data node.
 * @returns: pointer to new node.
*/
struct llist_node *llist_node_string_data_new()
{
    TRACE_ENTER(__func__)

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));
    node->data = (struct string_data *)malloc_zero(1, sizeof(struct string_data));

    node->id = next_llist_node_id;
    next_llist_node_id++;

    TRACE_LEAVE(__func__)

    return node;
}

/**
 * Allocates memory for text and copies {@code len} characters from
 * source text.
 * @param sd: container.
 * @param text: text string to copy.
 * @param len: number of characters to copy.
*/
void set_string_data(struct string_data *sd, char *text, size_t len)
{
    TRACE_ENTER(__func__)

    // string is always non-null but can be empty, is that a problem?
    sd->text = (char*)malloc_zero(1, len + 1);
    memcpy(sd->text, text, len);
    // no need for explicit '\0' since memcpy is one less than malloc_zero
    sd->len = len;

    TRACE_LEAVE(__func__)
}

/**
 * Frees memory for text pointer, and for base object.
 * @param sd: container.
*/
void string_data_free(struct string_data *sd)
{
    TRACE_ENTER(__func__)

    if (sd == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    if (sd->text != NULL)
    {
        free(sd->text);
        sd->text = NULL;
    }

    free(sd);

    TRACE_LEAVE(__func__)
}

/**
 * Iterates a list of {@code struct string_data} and prints the text contents.
*/
void llist_node_string_data_print(struct llist_root *root)
{
    TRACE_ENTER(__func__)

    struct llist_node *node = root->root;
    while (node != NULL)
    {
        struct string_data *sd = (struct string_data *)node->data;
        if (sd != NULL)
        {
            printf("%s\n", sd->text);
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Removes a node from the list and frees memory allocated to it.
 * This does not free or modify {@code node->data}.
 * Next and previous nodes in the list will have their pointers
 * updated in the expected manner.
 * If {@code root} is not NULL then the node count is decremented.
 * Otherwise the parent list the node belongs to will now have incorrect count.
 * If {@code node} is NULL then nothing happens.
 * @param root: list to remove node from. Optional. If not used, pass NULL.
 * @param node: node to remove from list.
*/
void llist_node_free(struct llist_root *root, struct llist_node *node)
{
    TRACE_ENTER(__func__)

    llist_node_detach(root, node);

    free(node);

    TRACE_LEAVE(__func__)
}

/**
 * Removes a node from the list but does not free memory.
 * This does not free or modify {@code node->data}.
 * Next and previous nodes in the list will have their pointers
 * updated in the expected manner.
 * If {@code root} is not NULL then the node count is decremented.
 * Otherwise the parent list the node belongs to will now have incorrect count.
 * If {@code node} is NULL then nothing happens.
 * @param root: list to remove node from. Optional. If not used, pass NULL.
 * @param node: node to remove from list.
*/
void llist_node_detach(struct llist_root *root, struct llist_node *node)
{
    TRACE_ENTER(__func__)

    if (node == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct llist_node *next = node->next;
    struct llist_node *prev = node->prev;
    int is_root = 0;
    int is_tail = 0;

    if (root != NULL)
    {
        is_root = node == root->root;
        is_tail = node == root->tail;
    }

    if (next != NULL)
    {
        next->prev = prev;
    }

    if (prev != NULL)
    {
        prev->next = next;
    }

    if (root != NULL)
    {
        if (root->count > 0)
        {
            root->count--;
        }

        if (is_root)
        {
            root->root = next;
        }

        if (is_tail)
        {
            root->tail = prev;
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Iterates list and frees all nodes.
 * This does not free or modify {@code node->data}.
 * The root node remains unchanged (not freed).
 * If root is NULL then nothing happens.
 * @param root: list to iterate and free child nodes.
*/
void llist_node_root_free_children(struct llist_root *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct llist_node *node = root->root;
    struct llist_node *next;

    while (node != NULL)
    {
        next = node->next;
        free(node);
        node = next;
    }

    root->root = NULL;
    root->tail = NULL;
    root->count = 0;

    TRACE_LEAVE(__func__)
}

/**
 * Iterates {@code struct string_data} list and frees only {@code node->data}
 * No other memory is freed.
 * If root is NULL then nothing happens.
 * @param root: list to iterate and free child nodes.
*/
void llist_node_free_string_data(struct llist_root *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct llist_node *node = root->root;

    while (node != NULL)
    {
        if (node->data != NULL)
        {
            struct string_data *sd = (struct string_data *)node->data;
            if (sd->text != NULL)
            {
                free(sd->text);
                sd->text = NULL;
            }

            free(node->data);
            node->data = NULL;
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Iterates list and frees all nodes.
 * This does not free or modify {@code node->data}.
 * The root node is then freed.
 * If root is NULL then nothing happens.
 * @param root: list to iterate and free.
*/
void llist_node_root_free(struct llist_root *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct llist_node *node = root->root;
    struct llist_node *next;

    while (node != NULL)
    {
        next = node->next;
        free(node);
        node = next;
    }

    root->root = NULL;
    root->tail = NULL;
    root->count = 0;

    free(root);

    TRACE_LEAVE(__func__)
}

/**
 * Only frees memory allocated to root, list elements
 * remain unchanged.
 * @param root: root element to free.
*/
void llist_node_root_free_only_self(struct llist_root *root)
{
    TRACE_ENTER(__func__)

    if (root == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    root->root = NULL;
    root->tail = NULL;
    root->count = 0;

    free(root);

    TRACE_LEAVE(__func__)
}

/**
 * Inserts a new node in the list immediately before the current node.
 * @param root: parent list. Optional, but required to update count.
 * @param current: current reference node.
 * @param to_insert: new node to insert.
*/
void llist_node_insert_before(struct llist_root *root, struct llist_node *current, struct llist_node *to_insert)
{
    TRACE_ENTER(__func__)

    if (current == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: current is NULL\n", __func__);
    }

    if (to_insert == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: to_insert is NULL\n", __func__);
    }

    if (to_insert == current)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: cant insert before self\n", __func__);
    }

    to_insert->next = current;
    to_insert->prev = current->prev;

    if (current->prev != NULL)
    {
        current->prev->next = to_insert;
    }

    current->prev = to_insert;

    if (root != NULL)
    {
        root->count++;

        if (current == root->root)
        {
            root->root = to_insert;
        }
    }

    TRACE_LEAVE(__func__)
}

/**
 * Swaps two nodes, updating all pointers accordingly.
 * @param first: first node to swap.
 * @param second: second node to swap.
*/
void llist_node_swap(struct llist_node *first, struct llist_node *second)
{
    TRACE_ENTER(__func__)

    if (first == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: first is NULL\n", __func__);
    }

    if (second == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: second is NULL\n", __func__);
    }

    if (first == second)
    {
        TRACE_LEAVE(__func__)
        return;
    }

    struct llist_node *first_prev = first->prev;
    struct llist_node *first_next = first->next;
    struct llist_node *second_prev = second->prev;
    struct llist_node *second_next = second->next;

    first->prev = second_prev;
    first->next = second_next;

    second->prev = first_prev;
    second->next = first_next;

    if (first_prev != NULL)
    {
        first_prev->next = second;
    }

    if (first_next != NULL)
    {
        first_next->prev = second;
    }

    if (second_prev != NULL)
    {
        second_prev->next = first;
    }

    if (second_next != NULL)
    {
        second_next->prev = first;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Detaches a node from one list and appends it to the end of another.
 * List counts and node pointers are updated accordingly.
 * No memory is allocated or freed.
 * @param dest: destination list to append to.
 * @param src: source list to remove from.
 * @param node: node to mode.
*/
void llist_node_move(struct llist_root *dest, struct llist_root *src, struct llist_node *node)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL\n", __func__, __LINE__);
    }

    if (src == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> src is NULL\n", __func__, __LINE__);
    }

    if (node == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> node is NULL\n", __func__, __LINE__);
    }

    llist_node_detach(src, node);
    llist_root_append_node(dest, node);

    TRACE_LEAVE(__func__)
}

/**
 * This filters a source list into a destination list.
 * Matching nodes are copied (shallow) from one list to the other.
 * Beware duplicate references to data (beware double free).
 * @param dest: Destination list to filter into. Must be previously allocated.
 * @param source: Source list to filter from.
 * @param filter_callback: Callback function which accepts a node and returns 1
 * if node should match, zero otherwise.
*/
void llist_root_where(struct llist_root *dest, struct llist_root *source, f_llist_node_filter filter_callback)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL\n", __func__, __LINE__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct llist_node *copy;
    int do_copy;

    node = source->root;
    while (node != NULL)
    {
        do_copy = 0;

        if (filter_callback == NULL)
        {
            do_copy = 1;
        }
        else
        {
            do_copy = filter_callback(node);
        }

        if (do_copy)
        {
            copy = llist_node_copy(node);
            llist_root_append_node(dest, copy);
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
* This filters a source list into a destination list.
 * Matching nodes are copied (shallow) from one list to the other.
 * Beware duplicate references to data (beware double free).
 * @param dest: Destination list to filter into. Must be previously allocated.
 * @param source: Source list to filter from.
 * @param filter_callback: Callback function which accepts a node and argument, and returns 1
 * if node should match, zero otherwise.
 * @param arg1: Integer argument to pass into callback.
*/
void llist_root_where_i(struct llist_root *dest, struct llist_root *source, f_llist_node_filter_i filter_callback, int arg1)
{
    TRACE_ENTER(__func__)

    if (dest == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> dest is NULL\n", __func__, __LINE__);
    }

    if (source == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> source is NULL\n", __func__, __LINE__);
    }

    if (filter_callback == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d> filter_callback is NULL\n", __func__, __LINE__);
    }

    struct llist_node *node;
    struct llist_node *copy;

    node = source->root;
    while (node != NULL)
    {
        if (filter_callback(node, arg1))
        {
            copy = llist_node_copy(node);
            llist_root_append_node(dest, copy);
        }

        node = node->next;
    }

    TRACE_LEAVE(__func__)
}

/**
 * Merge sort helper function.
 * Starting from head, iterates to the end of the list, splitting in to two lists.
 * For off length lists, the extra node goes into the first list.
 * @param head: first node.
 * @param firstptr: out parameter. Will contain pointer to first node in first half of the list (head node).
 * @param secondptr: out parameter. Will contain pointer to first node in second half of the list.
*/
static void llist_node_split(struct llist_node *head, struct llist_node **firstptr, struct llist_node **secondptr)
{
    TRACE_ENTER(__func__)

    struct llist_node *fast;
    struct llist_node *slow;
   
    slow = head;
    fast = head->next;
   
    while (fast != NULL)
    {
        fast = fast->next;
        if (fast != NULL)
        {
            slow = slow->next;
            fast = fast->next;
        }
    }
   
    *firstptr = head;
    *secondptr = slow->next;
    slow->next = NULL;

    TRACE_LEAVE(__func__)
}

/**
 * Merge sort helper function.
 * Compares two nodes, recursively setting the next node.
 * Returns comparison "winner".
 * @param first: first node to compare
 * @param second: second node to compare
 * @param compare_callback: function that accepts two nodes and returns comparison result (1, 0, -1).
 * @returns: comparison winner.
*/
static struct llist_node *llist_node_merge(struct llist_node *first, struct llist_node *second, f_llist_node_compare compare_callback)
{
    TRACE_ENTER(__func__)

    struct llist_node *result;
   
    if (first == NULL && second != NULL)
    {
        TRACE_LEAVE(__func__)
        return second;
    }
    else if (first != NULL && second == NULL)
    {
        TRACE_LEAVE(__func__)
        return first;
    }
    else if (first == NULL && second == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s: second is NULL\n", __func__);
    }
   
    int compare = compare_callback(first, second);
   
    if (compare == 0 || compare == -1)
    {
        result = first;
        result->next = llist_node_merge(first->next, second, compare_callback);
    }
    else
    {
        result = second;
        result->next = llist_node_merge(first, second->next, compare_callback);
    }

    TRACE_LEAVE(__func__)
   
    return result;
}

/**
 * Merge sort helper.
 * Entry to sort.
 * @param headptr: reference to pointer to node at start of list to sort.
 * @param compare_callback: function that accepts two nodes and returns comparison result (1, 0, -1).
*/
static void llist_node_merge_sort(struct llist_node **headptr, f_llist_node_compare compare_callback)
{
    TRACE_ENTER(__func__)

    struct llist_node *head = *headptr;
    struct llist_node *firstptr;
    struct llist_node *secondptr;
   
    if (head == NULL || head->next == NULL)
    {
        TRACE_LEAVE(__func__)
        return;
    }
   
    llist_node_split(head, &firstptr, &secondptr);
   
    llist_node_merge_sort(&firstptr, compare_callback);
    llist_node_merge_sort(&secondptr, compare_callback);
   
    *headptr = llist_node_merge(firstptr, secondptr, compare_callback);

    TRACE_LEAVE(__func__)
}

/**
 * Sorts the list using merge sort.
 * @param root: list to sort.
 * @param compare_callback: function that accepts two nodes and returns comparison result
 *     (1, 0, -1). Value of -1 prefers the accepts the first node, value of zero means the nodes
 *     should be considered equal, and a value of 1 accepts the second node.
*/
void llist_root_merge_sort(struct llist_root *root, f_llist_node_compare compare_callback)
{
    TRACE_ENTER(__func__)

    struct llist_node *node;
    struct llist_node *prev;
   
    llist_node_merge_sort(&root->root, compare_callback);
   
    // pointers to `prev` and root->tail are now broken, iterate the list and fix those
    node = root->root;
    prev = NULL;
    while (node != NULL)
    {
        node->prev = prev;
        prev = node;
        node = node->next;
    }
   
    root->tail = prev;

    TRACE_LEAVE(__func__)
}

/**
 * Merge sort comparison function.
 * Sorts on `key`, not value!
 * Use this to sort smallest to largest.
 * @param first: first node
 * @param second: second node
 * @returns: comparison result
*/
int llist_node_KeyValue_compare_smaller_key(struct llist_node *first, struct llist_node *second)
{
    TRACE_ENTER(__func__)

    int ret;

    if (first == NULL && second == NULL)
    {
        ret = 0;
    }
    else if (first == NULL && second != NULL)
    {
        ret = 1;
    }
    else if (first != NULL && second == NULL)
    {
        ret = -1;
    }
    else
    {
        struct KeyValue *kvp_first = (struct KeyValue *)first->data;
        struct KeyValue *kvp_second = (struct KeyValue *)second->data;
       
        if (kvp_first == NULL && kvp_second == NULL)
        {
            ret = 0;
        }
        else if (kvp_first == NULL && kvp_second != NULL)
        {
            ret = 1;
        }
        else if (kvp_first != NULL && kvp_second == NULL)
        {
            ret = -1;
        }
        else
        {
            if (kvp_first->key < kvp_second->key)
            {
                ret = -1;
            }
            else if (kvp_first->key > kvp_second->key)
            {
                ret = 1;
            }
            else
            {
                ret = 0;
            }
        }
    }

    TRACE_LEAVE(__func__)

    return ret;
}