#ifndef _GAUDIO_LLIST_H_
#define _GAUDIO_LLIST_H_

#include <stdlib.h>

/**
 * Simple double linked list node.
*/
struct LinkedListNode
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
     * Unallocated local data.
    */
    int data_local;

    /**
     * Next node.
    */
    struct LinkedListNode *next;

    /**
     * Previous node.
    */
    struct LinkedListNode *prev;
};

/**
 * Simple double linked list root object.
*/
struct LinkedList
{
    /**
     * Number of nodes in the list.
    */
    size_t count;

    /**
     * First node in the list.
    */
    struct LinkedListNode *head;

    /**
     * Last node in the list.
    */
    struct LinkedListNode *tail;
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
 * (1, 0, -1). Value of -1 accepts the first node, value of zero means the nodes
 * should be considered equal, and a value of 1 accepts the second node.
*/
typedef int (*f_LinkedListNode_compare)(struct LinkedListNode *, struct LinkedListNode *);

/**
 * function that accepts single node and returns bool result. Value
 * of 1 indicates the node should be accepted, value of zero means
 * node should not be accepted.
*/
typedef int (*f_LinkedListNode_filter)(struct LinkedListNode *node);
typedef int (*f_LinkedListNode_filter_i)(struct LinkedListNode *node, int arg1);

struct LinkedListNode *LinkedListNode_new(void);
struct LinkedListNode *LinkedListNode_string_data_new(void);
struct LinkedList *LinkedList_new(void);
struct LinkedListNode *LinkedListNode_copy(struct LinkedListNode *source);

void LinkedList_append_node(struct LinkedList *root, struct LinkedListNode *node);
void LinkedList_free(struct LinkedList *root);
void LinkedList_free_children(struct LinkedList *root);
void LinkedList_free_only_self(struct LinkedList *root);
void LinkedListNode_free(struct LinkedList *root, struct LinkedListNode *node);
void LinkedListNode_detach(struct LinkedList *root, struct LinkedListNode *node);
void LinkedListNode_move(struct LinkedList *dest, struct LinkedList *src, struct LinkedListNode *node);
void LinkedListNode_insert_before(struct LinkedList *root, struct LinkedListNode *current, struct LinkedListNode *to_insert);
void LinkedListNode_swap(struct LinkedList *root, struct LinkedListNode *first, struct LinkedListNode *second);

void LinkedListNode_free_string_data(struct LinkedList *root);
void LinkedListNode_string_data_print(struct LinkedList *root);
void set_string_data(struct string_data *sd, char *text, size_t len);
void string_data_free(struct string_data *sd);

void LinkedList_where(struct LinkedList *dest, struct LinkedList *source, f_LinkedListNode_filter filter_callback);
void LinkedList_where_i(struct LinkedList *dest, struct LinkedList *source, f_LinkedListNode_filter_i filter_callback, int arg1);
int LinkedList_any(struct LinkedList *source, f_LinkedListNode_filter filter_callback);
void LinkedList_merge_sort(struct LinkedList *root, f_LinkedListNode_compare compare_callback);
int LinkedListNode_KeyValue_compare_smaller_key(struct LinkedListNode *first, struct LinkedListNode *second);
#endif