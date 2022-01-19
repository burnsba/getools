#include <stdio.h>
#include <string.h>
#include "debug.h"
#include "machine_config.h"
#include "utility.h"
#include "llist.h"

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
    TRACE_ENTER("llist_root_append_node")

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
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_root_append_node: tail is NULL\n");
        }

        node->prev = root->tail;
        root->tail->next = node;
        root->tail = node;
    }

    TRACE_LEAVE("llist_root_append_node")
}

/**
 * Allocates memory for a new node.
 * @returns: pointer to new node.
*/
struct llist_node *llist_node_new()
{
    TRACE_ENTER("llist_node_new")

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));

    node->id = next_llist_node_id;
    next_llist_node_id++;

    TRACE_LEAVE("llist_node_new")

    return node;
}

/**
 * Allocates memory for a new root.
 * @returns: pointer to new root.
*/
struct llist_root *llist_root_new()
{
    TRACE_ENTER("llist_root_new")

    struct llist_root *root = (struct llist_root *)malloc_zero(1, sizeof(struct llist_root));

    TRACE_LEAVE("llist_root_new")

    return root;
}

/**
 * Allocates memory for a new node, and for a {@code struct string_data} data node.
 * @returns: pointer to new node.
*/
struct llist_node *llist_node_string_data_new()
{
    TRACE_ENTER("llist_node_string_data_new")

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));
    node->data = (struct string_data *)malloc_zero(1, sizeof(struct string_data));

    node->id = next_llist_node_id;
    next_llist_node_id++;

    TRACE_LEAVE("llist_node_string_data_new")

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
    TRACE_ENTER("set_string_data")

    // string is always non-null but can be empty, is that a problem?
    sd->text = (char*)malloc_zero(1, len + 1);
    memcpy(sd->text, text, len);
    // no need for explicit '\0' since memcpy is one less than malloc_zero
    sd->len = len;

    TRACE_LEAVE("set_string_data")
}

/**
 * Frees memory for text pointer, and for base object.
 * @param sd: container.
*/
void string_data_free(struct string_data *sd)
{
    TRACE_ENTER("string_data_free")

    if (sd == NULL)
    {
        return;
    }

    if (sd->text != NULL)
    {
        free(sd->text);
        sd->text = NULL;
    }

    free(sd);

    TRACE_LEAVE("string_data_free")
}

/**
 * Iterates a list of {@code struct string_data} and prints the text contents.
*/
void llist_node_string_data_print(struct llist_root *root)
{
    TRACE_ENTER("llist_node_string_data_print")

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

    TRACE_LEAVE("llist_node_string_data_print")
}

/**
 * Frees a node from the list.
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
    TRACE_ENTER("llist_node_free")

    if (node == NULL)
    {
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

    free(node);

    if (root != NULL)
    {
        if (root->count > 0)
        {
            root->count--;
        }

        if (is_root)
        {
            root->root = NULL;
        }

        if (is_tail)
        {
            root->tail = NULL;
        }
    }

    TRACE_LEAVE("llist_node_free")
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
    TRACE_ENTER("llist_node_root_free_children")

    if (root == NULL)
    {
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

    TRACE_LEAVE("llist_node_root_free_children")
}

/**
 * Iterates {@code struct string_data} list and frees only {@code node->data}
 * No other memory is freed.
 * If root is NULL then nothing happens.
 * @param root: list to iterate and free child nodes.
*/
void llist_node_free_string_data(struct llist_root *root)
{
    TRACE_ENTER("llist_node_free_string_data")

    if (root == NULL)
    {
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

    TRACE_LEAVE("llist_node_free_string_data")
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
    TRACE_ENTER("llist_node_root_free")

    if (root == NULL)
    {
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

    TRACE_LEAVE("llist_node_root_free")
}

/**
 * Inserts a new node in the list immediately before the current node.
 * @param root: parent list. Optional, but required to update count.
 * @param current: current reference node.
 * @param to_insert: new node to insert.
*/
void llist_node_insert_before(struct llist_root *root, struct llist_node *current, struct llist_node *to_insert)
{
    TRACE_ENTER("llist_node_insert_before")

    if (current == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_node_insert_before: current is NULL\n");
    }

    if (to_insert == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_node_insert_before: to_insert is NULL\n");
    }

    if (to_insert == current)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_node_insert_before: cant insert before self\n");
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
    }

    TRACE_LEAVE("llist_node_insert_before")
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
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_node_swap: first is NULL\n");
    }

    if (second == NULL)
    {
        stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "llist_node_swap: second is NULL\n");
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

    TRACE_LEAVE("llist_node_swap")
}

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