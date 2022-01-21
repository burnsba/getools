#ifndef _GAUDIO_LLIST_H_
#define _GAUDIO_LLIST_H_

#include <stdlib.h>

/**
 * Simple double linked list node.
*/
struct llist_node
{
    /**
     * Internal id.
    */
    int32_t id;

    /**
     * Pointer to node data.
    */
    void *data;

    /**
     * Next node.
    */
    struct llist_node *next;

    /**
     * Previous node.
    */
    struct llist_node *prev;
};

/**
 * Simple double linked list root object.
*/
struct llist_root
{
    /**
     * Number of nodes in the list.
    */
    size_t count;

    /**
     * First node in the list.
    */
    struct llist_node *root;

    /**
     * Last node in the list.
    */
    struct llist_node *tail;
};

/**
 * Simple text data node, to be used with the linked list node above.
*/
struct string_data
{
    /**
     * Length of text string.
    */
    size_t len;

    /**
     * Pointer to text.
    */
    char *text;
};

/**
 * function that accepts two nodes and returns comparison result
 * (1, 0, -1). Value of -1 prefers the accepts the first node, value of zero means the nodes
 * should be considered equal, and a value of 1 accepts the second node.
*/
typedef int (*f_llist_node_compare)(struct llist_node *, struct llist_node *);

void llist_root_append_node(struct llist_root *root, struct llist_node *node);
struct llist_node *llist_node_new();
struct llist_node *llist_node_string_data_new();
struct llist_root *llist_root_new();
void set_string_data(struct string_data *sd, char *text, size_t len);
void string_data_free(struct string_data *sd);
void llist_node_string_data_print(struct llist_root *root);
void llist_node_root_free_children(struct llist_root *root);
void llist_node_root_free(struct llist_root *root);
void llist_node_free_string_data(struct llist_root *root);
void llist_node_free(struct llist_root *root, struct llist_node *node);
void llist_node_insert_before(struct llist_root *root, struct llist_node *current, struct llist_node *to_insert);
void llist_node_swap(struct llist_node *first, struct llist_node *second);

void llist_root_merge_sort(struct llist_root *root, f_llist_node_compare compare_callback);

#endif