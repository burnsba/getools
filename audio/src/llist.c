#include <stdio.h>
#include <string.h>
#include "debug.h"
#include "utility.h"
#include "llist.h"

void llist_root_append_node(struct llist_root *root, struct llist_node *node)
{
    TRACE_ENTER("llist_root_append_node");

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
            fprintf(stderr, "llist_root_append_node: tail is NULL\n");
            fflush(stderr);
            exit(1);
        }

        node->prev = root->tail;
        root->tail->next = node;
        root->tail = node;
    }

    TRACE_LEAVE("llist_root_append_node");
}

struct llist_node *llist_node_string_data_new()
{
    TRACE_ENTER("llist_node_string_data_new");

    struct llist_node *node = (struct llist_node *)malloc_zero(1, sizeof(struct llist_node));
    node->data = (struct string_data *)malloc_zero(1, sizeof(struct string_data));

    TRACE_LEAVE("llist_node_string_data_new");

    return node;
}

void set_string_data(struct string_data *sd, char *text, size_t len)
{
    TRACE_ENTER("set_string_data");

    // string is always non-null but can be empty, is that a problem?
    sd->text = (char*)malloc_zero(1, len + 1);
    memcpy(sd->text, text, len);
    sd->text[len] = '\0';
    sd->len = len;

    TRACE_LEAVE("set_string_data");
}

void llist_node_string_data_print(struct llist_root *root)
{
    TRACE_ENTER("llist_node_string_data_print");

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

    TRACE_LEAVE("llist_node_string_data_print");
}