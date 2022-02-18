#include "ultra64.h"
#include "bondtypes.h"

// forward declarations
struct pad padlist[];
struct pad3d pad3dlist[];
s32 objlist[];
s32 intro[];
struct s_pathLink pathlist[];
char *pad3dnames[];
struct s_pathTbl pathtbl[];
char *padnames[];
struct s_pathSet paths[];
struct ailist ailists[];

struct stagesetup setup = {
    &pathtbl,
    &pathlist,
    &intro,
    &objlist,
    &paths,
    &ailists,
    &padlist,
    &pad3dlist,
    &padnames,
    &pad3dnames
};

struct pad padlist[] = {
    { {-389.0f, 95.0f, 160.0f}, {0.0f, 1.0f, 0.0f}, {-0.999859f, 0.0f, -0.016799f}, "p274a2", 0 },
    { {0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f}, NULL, 0 }
};


struct pad3d pad3dlist[] = {
    { {380.0f, 95.0f, -127.0f}, {-1.0f, 0.0f, 1e-06}, {0.0f, 1.0f, 0.0f}, "p25a2", 0, {-89.0f, 90.0f, -5.7e-05, 5.6e-05, -94.99999f, 93.99999f} },
    { {0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f}, {0.0f, 0.0f, 0.0f}, NULL, 0, {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f} }
};


s32 objlist[] = {
    /* Type = Collectable; index = 0 */
    _mkword(256, _mkshort(0, 8)), _mkword(333, 1), 0x00000001, 0x00000000, 0x00000000, 0x00000000, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x00000000, 0x00000000, 0, 0, _mkword(1000, 0), 0, 0, _mkword(_mkshort(0x58, 0xff), 0xffff), 0,
    /* Type = EndProps; index = 71 */
    _mkword(0, _mkshort(0, 48))
};


s32 intro[] = {
    /* Type = Spawn; index = 0 */
    _mkword(0, _mkshort(0, 0)), 0, 0,
    /* Type = EndIntro; index = 10 */
    _mkword(0, _mkshort(0, 9))
};


s32 path_neighbors_0[] = { 1, -1 };
s32 path_indeces_0[] = { 43, 44, 45, -1 };


struct s_pathLink pathlist[] = {
    { &path_neighbors_0, &path_indeces_0, 0 },
    { NULL, NULL, 0 }
};


char *pad3dnames[] = {
    "chk_locdam_all_p",
    NULL
};

s32 path_table_0[] = { 5, 39, 41, 42, -1 };


struct s_pathTbl pathtbl[] = {
    { 0x00000149, &path_table_0, 0x00000000, 0x00000000 },
    { 0xffffffff, NULL, 0x00000000, 0x00000000 }
};


char *padnames[] = {
    "PAD_mk44_dam_all_p",
    NULL
};

s32 path_set_0[] = { 148, 149, 150, -1 };


struct s_pathSet paths[] = {
    { &path_set_0, 0x00, 0x01, 0x0000 },
    { NULL, 0x00, 0x00, 0x0000 }
};


u32 ai_not_used_0[] = { 0x04000000 };

u32 ai_0[] = { 0x200005fd, 0x00070400 };


struct ailist ailists[] = {
    /* index = 0 */
    { &ai_0, 0x00000401 },
    /* index = 1 */
    { NULL, 0x00000000 }
};








