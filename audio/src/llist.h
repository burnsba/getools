#ifndef _LLIST_H_
#define _LLIST_H_

#include <stdlib.h>

struct llist_node
{
    void *data;
    struct llist_node *next;
    struct llist_node *prev;
};

struct llist_root
{
    size_t count;
    struct llist_node *root;
    struct llist_node *tail;
};

// 

struct string_data
{
    size_t len;
    char *text;
};

void llist_root_append_node(struct llist_root *root, struct llist_node *node);
struct llist_node *llist_node_new();
struct llist_node *llist_node_string_data_new();
void set_string_data(struct string_data *sd, char *text, size_t len);
void llist_node_string_data_print(struct llist_root *root);
void llist_node_free();
void llist_node_free(struct llist_root *root, struct llist_node *node);

#endif