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

void llist_root_append_node(struct llist_root *root, struct llist_node *node);
struct llist_node *llist_node_new();
struct llist_node *llist_node_string_data_new();
void set_string_data(struct string_data *sd, char *text, size_t len);
void string_data_free(struct string_data *sd);
void llist_node_string_data_print(struct llist_root *root);
void llist_node_free(struct llist_root *root, struct llist_node *node);
void llist_node_root_free_children(struct llist_root *root);
void llist_node_root_free(struct llist_root *root);
void llist_node_free_string_data(struct llist_root *root);
void llist_node_free(struct llist_root *root, struct llist_node *node);

#endif