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

static int test_where_filter_list(struct LinkedListNode *node);
static int test_where_int_filter_list(struct LinkedListNode *node, int arg1);

void linked_list_all(int *run_count, int *pass_count, int *fail_count)
{
    {
        printf("llist test: append\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedList *root = LinkedList_new();
        struct LinkedListNode *first = LinkedListNode_new();

        LinkedList_append_node(root, first);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        check = 1;
        check &= root->count == 1;
        check &= root->head == root->tail;
        check &= root->tail != NULL;
        check &= root->tail == first;

        struct LinkedListNode *second = LinkedListNode_new();
        LinkedList_append_node(root, second);

        check &= root->count == 2;
        check &= root->head == first;
        check &= root->tail != NULL;
        check &= root->tail == second;

        if (root->head == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head is NULL\n", __func__, __LINE__);
        }

        if (root->tail == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->tail is NULL\n", __func__, __LINE__);
        }

        if (first == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: first is NULL\n", __func__, __LINE__);
        }

        if (second == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: second is NULL\n", __func__, __LINE__);
        }

        check &= root->head->next == second;
        check &= root->tail->prev == first;
        check &= first->next == second;
        check &= second->prev == first;

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: insert before \n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedList *root = LinkedList_new();
        struct LinkedListNode *first = LinkedListNode_new();

        check = 1;

        LinkedList_append_node(root, first);

        struct LinkedListNode *second = LinkedListNode_new();
        LinkedList_append_node(root, second);

        struct LinkedListNode *node_a = LinkedListNode_new();
        LinkedList_append_node(root, node_a);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (second == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: second is NULL\n", __func__, __LINE__);
        }

        if (node_a == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: node_a is NULL\n", __func__, __LINE__);
        }

        check &= second->next == node_a;
        check &= node_a->prev == second;

        struct LinkedListNode *third = LinkedListNode_new();
        LinkedListNode_insert_before(root, node_a, third);

        if (third == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: third is NULL\n", __func__, __LINE__);
        }

        check &= root->count == 4;
        
        check &= root->tail == node_a;
        check &= node_a->next == NULL;

        check &= third->next == node_a;
        check &= node_a->prev == third;

        check &= second->next == third;
        check &= third->prev == second;

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: insert before root \n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedList *root = LinkedList_new();
        struct LinkedListNode *first = LinkedListNode_new();

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        check = 1;

        check &= root->head == NULL;

        LinkedList_append_node(root, first);

        check &= root->head == first;

        struct LinkedListNode *second = LinkedListNode_new();
        LinkedList_append_node(root, second);

        check &= root->head == first;
        check &= root->tail == second;

        struct LinkedListNode *node_a = LinkedListNode_new();
        LinkedListNode_insert_before(root, root->head, node_a);

        if (first == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: first is NULL\n", __func__, __LINE__);
        }

        if (second == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: second is NULL\n", __func__, __LINE__);
        }

        if (node_a == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: node_a is NULL\n", __func__, __LINE__);
        }

        check &= root->head == node_a;
        check &= node_a->next == first;
        check &= first->next == second;
        check &= root->tail == second;

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: sort\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedListNode *node;
        struct TestKeyValue *tkvp;

        struct LinkedList *root = LinkedList_new();

        node = LinkedListNode_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 2;
        node->data = tkvp;
        LinkedList_append_node(root, node);

        node = LinkedListNode_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 0;
        node->data = tkvp;
        LinkedList_append_node(root, node);

        node = LinkedListNode_new();
        tkvp = TestKeyValue_new();
        tkvp->key = 1;
        node->data = tkvp;
        LinkedList_append_node(root, node);

        LinkedList_merge_sort(root, LinkedListNode_TestKeyValue_compare_smaller_key);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (root->head == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head is NULL\n", __func__, __LINE__);
        }

        if (root->head->data == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head->data is NULL\n", __func__, __LINE__);
        }

        if (root->head->next == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head->next is NULL\n", __func__, __LINE__);
        }

        if (root->head->next->data == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head->next->data is NULL\n", __func__, __LINE__);
        }

        if (root->head->next->next == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root->head->next->next is NULL\n", __func__, __LINE__);
        }

        check = 1;
        check &= root->count == 3;
        check &= (((struct TestKeyValue *)root->head->data)->key == 0);
        check &= (((struct TestKeyValue *)root->head->next->data)->key == 1);
        check &= (((struct TestKeyValue *)root->head->next->next->data)->key == 2);

        node = root->head;
        while (node != NULL)
        {
            TestKeyValue_free(node->data);
            node = node->next;
        }

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
    
    {
        printf("llist test: LinkedListNode_swap far\n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedList *root = LinkedList_new();
        struct LinkedListNode *first = LinkedListNode_new();

        LinkedList_append_node(root, first);

        struct LinkedListNode *second = LinkedListNode_new();
        LinkedList_append_node(root, second);

        struct LinkedListNode *node_a = LinkedListNode_new();
        LinkedList_append_node(root, node_a);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (first == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: first is NULL\n", __func__, __LINE__);
        }

        if (second == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: second is NULL\n", __func__, __LINE__);
        }

        if (node_a == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: node_a is NULL\n", __func__, __LINE__);
        }

        check = 1;
        check &= root->head == first;
        check &= root->tail == node_a;
        check &= first->prev == NULL;
        check &= first->next == second;
        check &= second->prev == first;
        check &= second->next == node_a;
        check &= node_a->prev == second;
        check &= node_a->next == NULL;

        LinkedListNode_swap(root, first, node_a);

        check &= root->head == node_a;
        check &= root->tail == first;
        check &= first->prev == second;
        check &= first->next == NULL;
        check &= second->prev == node_a;
        check &= second->next == first;
        check &= node_a->prev == NULL;
        check &= node_a->next == second;

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: LinkedListNode_swap adjacent \n");
        *run_count = *run_count + 1;
        int check = 0;
        struct LinkedList *root = LinkedList_new();
        struct LinkedListNode *first = LinkedListNode_new();

        LinkedList_append_node(root, first);

        struct LinkedListNode *second = LinkedListNode_new();
        LinkedList_append_node(root, second);

        struct LinkedListNode *node_a = LinkedListNode_new();
        LinkedList_append_node(root, node_a);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (first == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: first is NULL\n", __func__, __LINE__);
        }

        if (second == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: second is NULL\n", __func__, __LINE__);
        }

        if (node_a == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: node_a is NULL\n", __func__, __LINE__);
        }

        check = 1;
        check &= root->head == first;
        check &= root->tail == node_a;
        check &= first->prev == NULL;
        check &= first->next == second;
        check &= second->prev == first;
        check &= second->next == node_a;
        check &= node_a->prev == second;
        check &= node_a->next == NULL;

        LinkedListNode_swap(root, first, second);

        check &= root->head == second;
        check &= root->tail == node_a;
        check &= first->prev == second;
        check &= first->next == node_a;
        check &= second->prev == NULL;
        check &= second->next == first;
        check &= node_a->prev == first;
        check &= node_a->next == NULL;

        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: where filter \n");
        *run_count = *run_count + 1;
        int check = 0;
        int i;
        struct LinkedList *root = LinkedList_new();
        struct LinkedList *filtered = LinkedList_new();
        struct LinkedListNode *node;

        for (i=0; i<32; i++)
        {
            node = LinkedListNode_new();
            node->data_local = i % 8;
            LinkedList_append_node(root, node);
        }

        LinkedList_where(filtered, root, test_where_filter_list);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (filtered == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: filtered is NULL\n", __func__, __LINE__);
        }

        check = 1;

        check &= filtered->count == 4;
        check &= root->count == 32;

        LinkedList_free(filtered);
        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }

    {
        printf("llist test: where filter arg \n");
        *run_count = *run_count + 1;
        int check = 0;
        int i;
        struct LinkedList *root = LinkedList_new();
        struct LinkedList *filtered = LinkedList_new();
        struct LinkedListNode *node;

        for (i=0; i<36; i++)
        {
            node = LinkedListNode_new();
            node->data_local = i % 8;
            LinkedList_append_node(root, node);
        }

        LinkedList_where_i(filtered, root, test_where_int_filter_list, 5);

        if (root == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: root is NULL\n", __func__, __LINE__);
        }

        if (filtered == NULL)
        {
            stderr_exit(EXIT_CODE_NULL_REFERENCE_EXCEPTION, "%s %d>: filtered is NULL\n", __func__, __LINE__);
        }

        check = 1;

        check &= filtered->count == 4;
        check &= root->count == 36;

        LinkedList_free_children(filtered);
        LinkedList_where_i(filtered, root, test_where_int_filter_list, 3);
        check &= filtered->count == 5;

        LinkedList_free_children(filtered);
        LinkedList_where_i(filtered, root, test_where_int_filter_list, 99);
        check &= filtered->count == 0;

        LinkedList_free(filtered);
        LinkedList_free(root);

        if (check == 1)
        {
            printf("pass\n");
            *pass_count = *pass_count + 1;
        }
        else
        {
            printf("%s %d> fail\n", __func__, __LINE__);
            *fail_count = *fail_count + 1;
        }
    }
}

static int test_where_filter_list(struct LinkedListNode *node)
{
    if (node == NULL)
    {
        return 0;
    }

    return node->data_local == 7;
}

static int test_where_int_filter_list(struct LinkedListNode *node, int arg1)
{
    if (node == NULL)
    {
        return 0;
    }

    return node->data_local == arg1;
}