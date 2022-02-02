#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "machine_config.h"
#include "debug.h"
#include "common.h"
#include "gaudio_math.h"
#include "utility.h"
#include "llist.h"
#include "string_hash.h"
#include "int_hash.h"
#include "md5.h"
#include "naudio.h"
#include "test_common.h"

void linked_list_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("llist test: append\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct llist_root *root = llist_root_new();
        struct llist_node *first = llist_node_new();

        llist_root_append_node(root, first);

        check = 1;
        check &= root->count == 1;
        check &= root->root == root->tail;
        check &= root->tail != NULL;
        check &= root->tail == first;

        struct llist_node *second = llist_node_new();
        llist_root_append_node(root, second);

        check &= root->count == 2;
        check &= root->root == first;
        check &= root->tail != NULL;
        check &= root->tail == second;
        check &= root->root->next == second;
        check &= root->tail->prev == first;
        check &= first->next == second;
        check &= second->prev == first;

        llist_node_root_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: insert before\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct llist_root *root = llist_root_new();
        struct llist_node *first = llist_node_new();

        check = 1;

        llist_root_append_node(root, first);

        struct llist_node *second = llist_node_new();
        llist_root_append_node(root, second);

        struct llist_node *node_a = llist_node_new();
        llist_root_append_node(root, node_a);

        check &= second->next == node_a;
        check &= node_a->prev == second;

        struct llist_node *third = llist_node_new();
        llist_node_insert_before(root, node_a, third);

        check &= root->count == 4;
        
        check &= root->tail == node_a;
        check &= node_a->next == NULL;

        check &= third->next == node_a;
        check &= node_a->prev == third;

        check &= second->next == third;
        check &= third->prev == second;

        llist_node_root_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: sort\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct llist_node *node;
        struct TestKeyValue *tkvp;

        struct llist_root *root = llist_root_new();

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 2;
        node->data = tkvp;
        llist_root_append_node(root, node);

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 0;
        node->data = tkvp;
        llist_root_append_node(root, node);

        node = llist_node_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 1;
        node->data = tkvp;
        llist_root_append_node(root, node);

        llist_root_merge_sort(root, llist_node_TestKeyValue_compare_smaller_key);

        check = 1;
        check &= root->count == 3;
        check &= (((struct TestKeyValue *)root->root->data)->key == 0);
        check &= (((struct TestKeyValue *)root->root->next->data)->key == 1);
        check &= (((struct TestKeyValue *)root->root->next->next->data)->key == 2);

        node = root->root;
        while (node != NULL)
        {
            TestKeyValue_free(node->data);
            node = node->next;
        }

        llist_node_root_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d>fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}