"use strict"

const svgns = "http://www.w3.org/2000/svg";

// When user clicks "goto", if the zoom level is less than this then the map will be zoomed in.
const g_MinZoomToPan = 8.0;

// Current app state.
let g_UseMode = 0;

/** global app state enum.  
    */
const UseMode = Object.freeze({
    Disable: Symbol("disable"),
    Move: Symbol("move"),
    Ruler: Symbol("ruler"),
    Annotate: Symbol("annotate"),
});

/**
    * 0: not started.
    * 
    * guard aim limit
    * 10: started, ghost circle to place point
    * 11: origin placed, ghost line to place aim limit
    * 
    * circle
    * 20: started, ghost circle to place point
    * 21: origin placed, ghost line to set radius
    * 
    * noise
    * 30: started, ghost circle to place point
    * 
    * erase
    * 99
    */
let g_AnnotateState = 0;

// Track the previous point clicked on the map, in annotation mode.
let g_AnnotatePrevPoint = { x: 0, y: 0 };

/**
* 0: not started
* 1: started, no previous point
* 2: started, at least one point
*/
let g_RulerDropMode = 0;
let g_LastRulerDropMode = 0;

// Track the previous point clicked on the map, in ruler mode.
let g_RulerPrevPoint = { "x": 0, "y": 0 };
// ruler
let g_TotalDistance = 0;

// ruler dot
// half width + entire border width
const g_DotRadius = 22;
const g_DotDiameter = g_DotRadius * 2;

const g_RulerLineDash = "40,20";
const g_RulerLineThickness = "18px"; // including unit suffix

// Current stage bounds of loaded map
var g_StageBounds = {
    "natural": {
        "min": {
            "x": 0.0,
            "y": 0.0,
            "z": 0.0
        },
        "max": {
            "x": 0.0,
            "y": 0.0,
            "z": 0.0
        }
    },
    "scaled": {
        "min": {
            "x": 0.0,
            "y": 0.0,
            "z": 0.0
        },
        "max": {
            "x": 0.0,
            "y": 0.0,
            "z": 0.0
        }
    }
};

// Flag to indicate if any stage has ever been loaded
var g_ZoomerLoaded = false;

var g_SvgStyleSheet = null;
var g_SvgStyleRuleIndexLookup = {};
var g_SvgStyleCurrentValues = {
    /* default/starting values */
    '.gelib-stan': {
        'stroke': '#9e87a3',
        'stroke-width': 1,
        'fill': '#fdf5ff'
    },
    '.gelib-stan:hover': {
        'stroke': '#9e87a3',
        'stroke-width': 4,
        'fill': '#fdf5ff'
    },
    '.gelib-room': {
        'stroke': 'blue',
        'stroke-width': 2,
        'fill': '#ffffff',
        'fill-opacity': 0
    },
    '.gelib-room:hover': {
        'stroke': 'blue',
        'stroke-width': 6,
        'fill': '#ffffff'
    },
    '.gelib-pad': {
        'stroke': '#9c009e',
        'stroke-width': 2,
        'fill': '#ff00ff'
    },
    '.gelib-pad:hover': {
        'stroke': '#ec60ee',
        'stroke-width': 4,
        'fill': '#ff99ff'
    },
    '.gelib-wpline': {
        'stroke': '#ff80ff',
        'stroke-width': 12
    },
    '.gelib-wpline:hover': {
        'stroke': '#a831a8',
        'stroke-width': 18
    },
    '.gelib-patrol': {
        'stroke': '#3fb049',
        'stroke-width': 18
    },
    '.gelib-patrol:hover': {
        'stroke': '#94e34f',
        'stroke-width': 24
    }
};

// current dialog object
let g_DialogBox = null;

// Global flag to enable/disable dialog box
var g_AllowDialog = true;

// Currently selected item. dom node in svg container.
var g_SelectedItem = null;

// Flag to indicate if vertical slider has been initialized.
var g_SliderInit = false;

// "temp" variable, global object used to map click events on the svg to scaled coordinates.
var g_SvgPoint = null;

// maps svg id name to getools c# type.
var ClassTypeMap = [
    /* need longer matches first */
    { "re": new RegExp("svg-setup-alarm-([0-9]+)"), 'id': 15, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectAlarm', 'display': 'Alarm' },
    { "re": new RegExp("svg-setup-ammo-([0-9]+)"), 'id': 14, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectAmmoBox', 'display': 'Ammo' },
    { "re": new RegExp("svg-setup-aircraft-([0-9]+)"), 'id': 13, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectAircraft', 'display': 'Aircraft' },
    { "re": new RegExp("svg-setup-bodyarmor-([0-9]+)"), 'id': 12, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectBodyArmor', 'display': 'Body Armor' },
    { "re": new RegExp("svg-setup-chr-([0-9]+)"), 'id': 11, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectGuard', 'display': 'Guard' },
    { "re": new RegExp("svg-setup-cctv-([0-9]+)"), 'id': 10, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectCctv', 'display': 'CCTV' },
    { "re": new RegExp("svg-setup-collectable-([0-9]+)"), 'id': 9, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectCollectObject', 'display': 'Collect' },
    { "re": new RegExp("svg-setup-door-([0-9]+)"), 'id': 8, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectDoor', 'display': 'Door' },
    { "re": new RegExp("svg-setup-drone-([0-9]+)"), 'id': 7, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectDrone', 'display': 'Drone' },
    { "re": new RegExp("svg-setup-key-([0-9]+)"), 'id': 6, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectKey', 'display': 'Key' },
    { "re": new RegExp("svg-setup-safe-([0-9]+)"), 'id': 5, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectSafe', 'display': 'Safe' },
    { "re": new RegExp("svg-setup-singlemonitor-([0-9]+)"), 'id': 4, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectSingleMonitor', 'display': 'SingleMonitor' },
    { "re": new RegExp("svg-setup-prop-([0-9]+)"), 'id': 3, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectStandard', 'display': 'Prop' },
    { "re": new RegExp("svg-setup-tank-([0-9]+)"), 'id': 2, 'type': 'Getools.Lib.Game.Asset.SetupObject.SetupObjectTank', 'display': 'Tank' },
    { "re": new RegExp("svg-setup-intro-([0-9]+)"), 'id': 1, 'type': 'Getools.Lib.Game.Asset.Intro.IntroSpawn', 'display': 'Spawn' },

    { "re": new RegExp("svg-patrol-([0-9]+)"), 'id': 19, 'type': 'Getools.Lib.Game.Asset.Setup.PathSet', 'display': 'Patrol' },

    { "re": new RegExp("svg-room-([0-9]+)-tile-([0-9]+)"), 'id': 17, 'type': 'Getools.Lib.Game.Asset.Stan.StandTile', 'display': 'Tile' },
    { "re": new RegExp("svg-pad-([0-9]+)"), 'id': 16, 'type': 'Getools.Lib.Game.Asset.Setup.Pad', 'display': 'Pad' },
    { "re": new RegExp("svg-room-([0-9]+)"), 'id': 18, 'type': 'Getools.Lib.Game.Asset.Bg.Room', 'display': 'Room' },
];

// Dialog box "simple mode" button handler.
function DialogCheckSimpleMode() {
    g_AllowDialog = false;
    SvgUnselectItems();
    HideDialog();
    document.getElementById('ui-panel-simple-mode-check').checked = true;
}

// Checkbox event handler. Toggle "simple mode".
function UiPanelToggleSimpleMode(node) {
    if (node.checked) {
        g_AllowDialog = false;
        SvgUnselectItems();
        HideDialog();
    } else {
        g_AllowDialog = true;
    }
}

// Hides/closes the dialog window.
function HideDialog() {
    document.getElementById('dialog').style.display = 'none';
}

// Applies or re-applies the current style definition in the svg document.
function ApplyStyleChange(node) {
    if (!g_ZoomerLoaded || !node) {
        return;
    }

    let selector = node.dataset.selector;
    let property = node.dataset.prop;
    let css_type = node.dataset.cssType;

    if (!selector || !property || !css_type) {
        return;
    }

    if (!g_SvgStyleSheet || !g_SvgStyleCurrentValues[selector]) {
        return;
    }

    let svg_style_index = g_SvgStyleRuleIndexLookup[selector];

    if (!isFinite(svg_style_index)) {
        return;
    }

    let prop_value = null;

    if (css_type === 'number') {
        prop_value = parseFloat(node.value);
    } else if (css_type === 'color') {
        // match 6 or 3 digit html hex, with optional '#' prefix
        let rem = ('' + node.value).match(/^#?[0-9a-fA-F]{6}$|^#?[0-9a-fA-F]{3}$/);
        if (!rem) {
            return;
        }
        prop_value = rem[0];
    } else {
        return;
    }

    let rules = g_SvgStyleSheet.cssRules || g_SvgStyleSheet.rules;
    rules[svg_style_index].style[property] = prop_value;

    g_SvgStyleCurrentValues[selector][property] = prop_value;
}

// Sets initial values for the style input section.
function SetInitialControlValues() {
    let style_node = document.getElementById('style-panel');
    let inputs = style_node.getElementsByTagName('input');
    for (const node of inputs) {
        let selector = node.dataset.selector;
        if (!selector) {
            continue;
        }
        let style_values = g_SvgStyleCurrentValues[selector];
        if (!style_values) {
            continue;
        }
        let property = node.dataset.prop;
        node.value = style_values[property];
    }
}

// Injects the selected svg filename into the svg container.
// Setting the "data" attribute here triggers the load event.
function ChangeLevel() {
    const node = document.getElementById('level-select');
    if (!node) {
        return;
    }
    const options = node.options;
    if (!options) {
        return;
    }
    const selectedOption = options[options.selectedIndex];
    if (!selectedOption) {
        return;
    }
    let filename = selectedOption.dataset['filename'];

    if (!filename) {
        return;
    }

    document.getElementById('mmap').setAttribute("data", filename);
}

// Event handler for checkbox that hides/unhides individual layers.
function ToggleLayer(node) {
    let svg_layer_id = "";

    switch (node.name) {
        case "check-bg-layer": svg_layer_id = "svg-bg-room-layer"; break;
        case "check-stan-layer": svg_layer_id = "svg-stan-tile-layer"; break;
        case "check-setup-pad-layer": svg_layer_id = "svg-pad-layer"; break;
        case "check-setup-waypoint-layer": svg_layer_id = "svg-waypoint-layer"; break;
        case "check-setup-patrol-layer": svg_layer_id = "svg-patrol-layer"; break;
        case "check-setup-alarm-layer": svg_layer_id = "svg-setup-alarm-layer"; break;
        case "check-setup-ammo-layer": svg_layer_id = "svg-setup-ammo-layer"; break;
        case "check-setup-aircraft-layer": svg_layer_id = "svg-setup-aircraft-layer"; break;
        case "check-setup-bodyarmor-layer": svg_layer_id = "svg-setup-bodyarmor-layer"; break;
        case "check-setup-chr-layer": svg_layer_id = "svg-setup-chr-layer"; break;
        case "check-setup-cctv-layer": svg_layer_id = "svg-setup-cctv-layer"; break;
        case "check-setup-collectable-layer": svg_layer_id = "svg-setup-collectable-layer"; break;
        case "check-setup-door-layer": svg_layer_id = "svg-setup-door-layer"; break;
        case "check-setup-drone-layer": svg_layer_id = "svg-setup-drone-layer"; break;
        case "check-setup-key-layer": svg_layer_id = "svg-setup-key-layer"; break;
        case "check-setup-safe-layer": svg_layer_id = "svg-setup-safe-layer"; break;
        case "check-setup-singlemonitor-layer": svg_layer_id = "svg-setup-singlemonitor-layer"; break;
        case "check-setup-prop-layer": svg_layer_id = "svg-setup-prop-layer"; break;
        case "check-setup-tank-layer": svg_layer_id = "svg-setup-tank-layer"; break;
        case "check-setup-intro-layer": svg_layer_id = "svg-setup-intro-layer"; break;

            break;
    }

    if (!svg_layer_id) {
        return;
    }

    var svg_doc = document.getElementById('mmap').contentDocument;
    if (!svg_doc) {
        return;
    }

    var svg_node = svg_doc.getElementById(svg_layer_id);
    if (!svg_node) {
        return;
    }

    if (!node.checked) {
        svg_node.style.display = 'none';
    } else {
        svg_node.style.display = '';
    }
}

// Main event handler to load stage data. Triggered on "load" event.
function HandleSvgLoadEvent() {
    svgPanZoom(document.getElementById('mmap'), {
        maxZoom: 100
    });

    g_ZoomerLoaded = true;

    SvgUnselectItems();
    ResetAndHideDialog();
    UpdateStageBounds();
    RebuildSlider();
    InjectDocumentStyle();
    BuildPropList();
    BuildPatrolList();
    BuildPadsList();
    BuildGuardsList();
    BuildAiScriptsList();
    InjectEventHandlers();
    ApplyLayerFilter();

    // loading the svg object un-applies the css width, manually reapply that here.
    FixMapDisplayStuff();

    SetSvgPoint();

    // make sure control buttons are no longer disabled.
    UiUpdateRulerControlButtons();
}

// Clears out state within the dialog window, then hides it.
function ResetAndHideDialog() {
    if (!g_DialogBox) {
        return;
    }

    HideDialog();
    DialogSetTitlebar("...");
    DialogClearSelected();
    DialogClearAi();
}

// Sets the value on the global g_SvgPoint.
function SetSvgPoint() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    g_SvgPoint = svg.createSVGPoint();
}

// Collapses the left panel.
function HideUiControls() {
    document.getElementById('column-left-body').style.display = 'none';
    document.getElementById('btn-hide-ui').style.display = 'none';
    document.getElementById('btn-show-ui').style.display = '';
    FixMapDisplayStuff(true);
}

// Expands the left panel.
function ShowUiControls() {
    document.getElementById('column-left-body').style.display = '';
    document.getElementById('btn-hide-ui').style.display = '';
    document.getElementById('btn-show-ui').style.display = 'none';
    FixMapDisplayStuff();
}

// Reapplies / fixes the width and offset of the svg container on stage load.
function FixMapDisplayStuff(v) {
    if (v) {
        document.getElementById('mmap').style.width = 'calc(100vw - 4px)';
        document.getElementById('mmap').style.left = 0;
    } else {
        document.getElementById('mmap').style.width = 'calc(100vw - 4px - 300px)';
        document.getElementById('mmap').style.left = '300px';
    }
}

// Takes all of the "hide/unhide" checkboxes and applies that state on the svg.
function ApplyLayerFilter() {
    let unchecked = document.getElementById('control-layers-panel').querySelectorAll('input:not(:checked)');
    for (const node of unchecked) {
        ToggleLayer(node);
    }
}

// The svg image does not inherit the page style, it must be manually injected into the svg.
// This method does that.
function InjectDocumentStyle() {
    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return null;
    }

    let svg = xml.getElementsByTagName('svg');
    if (!svg || svg.length != 1) {
        return null;
    }

    svg = svg[0];

    var style = xml.createElementNS(svgns, 'style');
    svg.appendChild(style);
    style.type = 'text/css';

    g_SvgStyleSheet = style.sheet;

    style.sheet.insertRule('.vertical-bound-filter { display: none; }', style.sheet.cssRules.length);
    style.sheet.insertRule('#fake-marker { position: absolute; opacity: 0.5; pointer-events: none; }', style.sheet.cssRules.length);
    style.sheet.insertRule('#fake-line { min-width: 120px; position: absolute; opacity: 0.5; pointer-events: none; }', style.sheet.cssRules.length);
    style.sheet.insertRule('.line { min-height: 0px; border-bottom: 6px dashed #000000; transform-origin: 0px 3px; }', style.sheet.cssRules.length);
    style.sheet.insertRule('.dot { background-color: #00ffc3; border: 2px solid #4580ff; border-radius: 50 %; }', style.sheet.cssRules.length);
    // style the selected item, including a transition effect when first selected.
    style.sheet.insertRule('#svg-setup-chr-layer .svg-logical-item.selected, #svg-pad-layer .svg-logical-item.selected {' +
            'outline: 14px solid rgba(255,0,0,0.5); ' +
            'outline-offset: 30px; ' +
            'outline-style: dashed; ' +
            'transition-property: outline-offset; ' +
            'transition-duration: 0.5s; ' +
            'transition-timing-function: linear; ' +
            ' } ', style.sheet.cssRules.length);
    // style secondary selected item, including a transition effect when first selected.
    style.sheet.insertRule('#svg-setup-chr-layer .svg-logical-item.secondary, #svg-pad-layer .svg-logical-item.secondary {' +
            'outline: 14px solid rgba(0,220,225,0.7); ' +
            'outline-offset: 30px; ' +
            'outline-style: dashed; ' +
            'transition-property: outline-offset; ' +
            'transition-duration: 0.5s; ' +
            'transition-timing-function: linear; ' +
            ' } ', style.sheet.cssRules.length);

    let index = 0;
    let rule = '';
    // replace the svg css styles with the values from the javascript css container.
    for (const selector of Object.entries(g_SvgStyleCurrentValues).map(key => key[0])) {
        rule = Array.prototype.join.call(Object.entries(g_SvgStyleCurrentValues[selector]).map(kvp => `${kvp[0]}: ${kvp[1]};`), ' ');
        let rule_text = selector + ' { ' + rule + ' }';
        index = style.sheet.insertRule(rule_text, style.sheet.cssRules.length);
        // save reference to allow user updating
        g_SvgStyleRuleIndexLookup[selector] = index;
    }
}

// Applies event handlers to the svg.
function InjectEventHandlers() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    svg.addEventListener('mousemove', HandleMouseMove);
    svg.addEventListener('click', HandleSvgClick);
    svg.addEventListener('touchstart', HandleSvgClick);
}

// Update global g_StageBounds from currently loaded svg.
function UpdateStageBounds() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    g_StageBounds.natural.min.x = parseFloat(svg.dataset.nMinX);
    g_StageBounds.natural.max.x = parseFloat(svg.dataset.nMaxX);
    g_StageBounds.natural.min.y = parseFloat(svg.dataset.nMinY);
    g_StageBounds.natural.max.y = parseFloat(svg.dataset.nMaxY);
    g_StageBounds.natural.min.z = parseFloat(svg.dataset.nMinZ);
    g_StageBounds.natural.max.z = parseFloat(svg.dataset.nMaxZ);

    g_StageBounds.scaled.min.x = parseFloat(svg.dataset.sMinX);
    g_StageBounds.scaled.max.x = parseFloat(svg.dataset.sMaxX);
    g_StageBounds.scaled.min.y = parseFloat(svg.dataset.sMinY);
    g_StageBounds.scaled.max.y = parseFloat(svg.dataset.sMaxY);
    g_StageBounds.scaled.min.z = parseFloat(svg.dataset.sMinZ);
    g_StageBounds.scaled.max.z = parseFloat(svg.dataset.sMaxZ);

    document.getElementById('map-min-y').innerHTML = g_StageBounds.scaled.min.y;
    document.getElementById('map-max-y').innerHTML = g_StageBounds.scaled.max.y;

    // console.log('new bounds: g_StageScaledMinY=' + g_StageBounds.scaled.min.y + ', g_StageScaledMaxY=' + g_StageBounds.scaled.max.y);
}

// iterate svg, extract unique prop ids, create UI checkbox input to allow disabling in left panel.
function BuildPropList() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let prop_names = [... new Set(Array.from(svg.querySelectorAll('[data-prop-name]')).map(x => x.dataset.propName))];
    let container = document.getElementById('show-hide-prop-inputs');

    container.innerHTML = '';

    for (const name of prop_names) {
        let div = document.createElement('div');
        div.classList.add("s-row");

        let label = document.createElement('label');
        let input = document.createElement('input');

        label.innerText = name;

        input.setAttribute('type', 'checkbox');
        input.dataset.propName = name;
        input.addEventListener('change', ToggleDisableProp);

        label.appendChild(input);
        div.appendChild(label);

        container.appendChild(div);
    }
}

// iterate svg, extract unique prop ids, create UI checkbox input to allow disabling in left panel.
function BuildPatrolList() {

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let patrolContainerNodes = [... new Set(Array.from(svg.querySelectorAll('.gelib-patrol')).map(x => x.closest('.svg-logical-item')))];
    let container = document.getElementById('show-hide-patrol-inputs');

    container.innerHTML = '';

    for (const node of patrolContainerNodes) {
        let div = document.createElement('div');
        div.classList.add("s-row");

        let label = document.createElement('label');
        let input = document.createElement('input');

        let rem = node.id.match(/^svg-patrol-([0-9]+)$/);
        if (!rem || rem.length < 2) {
            continue;
        }
        label.innerText = `patrol ${rem[1]}`;

        input.setAttribute('type', 'checkbox');
        input.dataset.patrolNodeId = node.id;
        node.dataset.patrolNodeId = rem[1];
        input.addEventListener('change', ToggleDisablePatrol);

        label.appendChild(input);
        div.appendChild(label);

        container.appendChild(div);
    }
}

// Iterate svg, extract unique ai script list for left panel.
function BuildAiScriptsList() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let aiNodes = [... new Set(Array.from(svg.querySelectorAll('#svg-ailists > g')))];
    let container = document.getElementById('list-ai-scripts');

    container.innerHTML = '';

    for (const node of aiNodes) {
        let rem = node.id.match(/^svg-ai-([0-9]+)$/);
        if (!rem || rem.length < 2) {
            continue;
        }

        let id = parseInt(rem[1]);

        let option = document.createElement('option');
        option.dataset.aiId = id;
        option.dataset.aiNodeId = node.id;
        option.textContent = `ai 0x${id.toString(16)}`;

        container.appendChild(option);
    }
}

// Iterate svg, extract guards for left panel.
function BuildGuardsList() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let aiNodes = [... new Set(Array.from(svg.querySelectorAll('#svg-setup-chr-layer > g')))];
    let container = document.getElementById('list-guards');

    container.innerHTML = '';

    for (const node of aiNodes) {
        let rem = node.id.match(/^svg-setup-chr-([0-9]+)$/);
        if (!rem || rem.length < 2) {
            continue;
        }

        let id = parseInt(rem[1]);

        let option = document.createElement('option');
        option.dataset.xChrId = id;
        option.textContent = `guard 0x${id.toString(16)}`;

        container.appendChild(option);
    }
}

// Iterate svg, extract pads for left panel.
function BuildPadsList() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let aiNodes = [... new Set(Array.from(svg.querySelectorAll('#svg-pad-layer > g')))];
    let container = document.getElementById('list-pads');

    container.innerHTML = '';

    for (const node of aiNodes) {
        let rem = node.id.match(/^svg-pad-([0-9]+)$/);
        if (!rem || rem.length < 2) {
            continue;
        }

        let id = parseInt(rem[1]);

        let option = document.createElement('option');
        option.dataset.xPadId = id;
        option.textContent = `pad 0x${id.toString(16)}`;

        container.appendChild(option);
    }
}

// Event handler, hides/unhides props for the associated checkbox arg.
function ToggleDisableProp(e) {
    if (!e) {
        return;
    }
    let node = e.target;
    if (!node) {
        return;
    }

    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return;
    }

    let prop_name = node.dataset.propName;
    let display_value = '';
    if (node.checked) {
        display_value = 'none';
    }

    Array.from(xml.querySelectorAll('[data-prop-name="' + prop_name + '"]')).forEach(x => x.style.display = display_value);
}

// Event handler, hides/unhides patrols for the associated checkbox arg.
function ToggleDisablePatrol(e) {
    if (!e) {
        return;
    }
    let node = e.target;
    if (!node) {
        return;
    }

    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return;
    }

    let patrolNodeId = node.dataset.patrolNodeId;
    let display_value = '';
    if (node.checked) {
        display_value = 'none';
    }

    Array.from(xml.querySelectorAll('#' + patrolNodeId)).forEach(x => x.style.display = display_value);
}

function HideAllProps() {
    ToggleVisibilityAllProps('hide');
}
function ShowAllProps() {
    ToggleVisibilityAllProps('show');
}
// Convenience function to handle all the hide/show logic for "all props".
function ToggleVisibilityAllProps(mode) {

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let display_value = '';
    let checkvalue = false;
    if (mode === 'hide') {
        display_value = 'none';
        checkvalue = true;
    } else if (mode === 'show') {
        // nothing to do
    } else {
        return;
    }

    // svg elements. Not all props are in the setup-prop-layer. [data-prop-name] is always .svg-logical-item
    Array.from(svg.querySelectorAll('[data-prop-name]')).forEach(x => x.style.display = display_value);
    // ui checkmark state
    Array.from(document.getElementById('show-hide-prop-inputs').querySelectorAll('input')).forEach(x => x.checked = checkvalue);
}

function HideAllPatrols() {
    ToggleVisibilityAllPatrols('hide');
}
function ShowAllPatrols() {
    ToggleVisibilityAllPatrols('show');
}
// Convenience function to handle all the hide/show logic for "all patrols".
function ToggleVisibilityAllPatrols(mode) {

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let display_value = '';
    let checkvalue = false;
    if (mode === 'hide') {
        display_value = 'none';
        checkvalue = true;
    } else if (mode === 'show') {
        // nothing to do
    } else {
        return;
    }

    // svg elements
    Array.from(svg.querySelectorAll('#svg-patrol-layer .svg-logical-item')).forEach(x => x.style.display = display_value);
    // ui checkmark state
    Array.from(document.getElementById('show-hide-patrol-inputs').querySelectorAll('input')).forEach(x => x.checked = checkvalue);
}

// Instantiates the dialog object if it doesn't already exist.
function EnsureDialog() {
    if (!g_DialogBox) {
        var id = 'dialog';
        g_DialogBox = new DialogBox(id, DefaultDialogCallback);
    }
}

function DefaultDialogCallback(btnName) {
    //
}

// Button event handler to go to selected pad.
function HandlePadListClick() {

    let select = document.getElementById('list-pads');
    if (select.selectedIndex < 0) {
        return;
    }

    let option = select.options[select.selectedIndex];
    if (!option) {
        return;
    }
    // the select option has dataset.xPadId, so we can reuse the dialog handle
    // with the appropriate `this`
    HandleDialogClickPanToPad.call(option);
}

// Button event handler to go to selected guard.
function HandleGuardListClick() {
    let select = document.getElementById('list-guards');
    if (select.selectedIndex < 0) {
        return;
    }

    let option = select.options[select.selectedIndex];
    if (!option) {
        return;
    }
    // the select option has dataset.xChrId, so we can reuse the dialog handle
    // with the appropriate `this`
    HandleDialogClickPanToChr.call(option);
}

// Button event handler to show the selected ai script.
function HandleAiListClick() {
    if (!g_AllowDialog) {
        return;
    }

    EnsureDialog();

    let select = document.getElementById('list-ai-scripts');
    if (select.selectedIndex < 0) {
        return;
    }

    let option = select.options[select.selectedIndex];
    if (!option) {
        return;
    }

    let aiId = parseInt(option.dataset.aiId);

    DialogLoadAiScriptContent(aiId);
    DialogSetTitlebar(`ai 0x${aiId.toString(16)}`);

    DialogClearSelected();

    g_DialogBox.showDialog();
}

// Given an index/id, returns the svg dom id for an ai script.
function AiIdToSvgNodeId(aiId) {
    let id = parseInt(aiId);
    return `svg-ai-${id}`;
}

// Loads ai script into dialog. Will add the "goto pad" and "goto chr" buttons
// if needed.
// param aiId: id/index of ai script.
// param prependTitleSeperator: If set, prepends the name of the ai script that is loaded.
//     This is used when a pad/chr is selected and the ai script is secondary.
function DialogLoadAiScriptContent(aiId, prependTitleSeperator) {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    aiId = parseInt(aiId);

    EnsureDialog();

    let dialogContent = document.getElementById('dialog-ailist');
    dialogContent.innerHTML = '';

    if (prependTitleSeperator) {
        DialogAppendSeperator(dialogContent);

        let div = document.createElement('div');
        div.innerText = `ai script: 0x${aiId.toString(16)}`;
        dialogContent.appendChild(div);
    }

    let svgid = AiIdToSvgNodeId(aiId);

    let svgAiNode = svg.querySelectorAll('#' + svgid)[0];

    for (const node of svgAiNode.childNodes) {
        if (node.textContent.match(/^\s*$/)) {
            continue;
        }

        let div = document.createElement('div');
        div.classList.add('dialog-ai-command');

        let divContent = document.createElement('div');
        divContent.innerText = node.textContent;
        div.appendChild(divContent);

        if (node.textContent.match(/^...: \[0x02\]/)) {
            div.classList.add('ai-label');
        }

        if (node.dataset.xPadId) {
            let id = parseInt(node.dataset.xPadId);

            let panToNode = document.createElement('input');
            panToNode.setAttribute("type", "button");
            panToNode.value = `goto pad 0x${id.toString(16)}`;
            panToNode.dataset.xPadId = id;
            panToNode.onclick = HandleDialogClickPanToPad;
            div.appendChild(panToNode);
        }

        if (node.dataset.xChrId) {
            let id = parseInt(node.dataset.xChrId);

            let target = svg.querySelectorAll(`#svg-setup-chr-layer #svg-setup-chr-${id}`);

            if (!target || target.length < 1) {
                let msgNode = document.createElement('div');
                msgNode.innerHTML = `LATE SPAWN: chr 0x${id.toString(16)}`;
                div.appendChild(msgNode);
            } else {
                let panToNode = document.createElement('input');
                panToNode.setAttribute("type", "button");
                panToNode.value = `goto chr 0x${id.toString(16)}`;
                panToNode.dataset.xChrId = id;
                panToNode.onclick = HandleDialogClickPanToChr;
                div.appendChild(panToNode);
            }
        }

        dialogContent.appendChild(div);
    }
}

// Event handler for the "goto pad" button in an ai script in the dialog.
function HandleDialogClickPanToPad(e) {
    let node = this;

    let id = parseInt(node.dataset.xPadId);

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let target = svg.querySelectorAll(`#svg-pad-layer #svg-pad-${id}`)[0];

    SvgUnselectSecondary();
    SpzPanToItem(target);
    SpzNodeAddReClass(target, "secondary");
}

// Event handler for the "goto chr" button in an ai script in the dialog.
function HandleDialogClickPanToChr(e) {
    let node = this;

    let id = parseInt(node.dataset.xChrId);

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let target = svg.querySelectorAll(`#svg-setup-chr-layer #svg-setup-chr-${id}`)[0];

    SvgUnselectSecondary();
    SpzPanToItem(target);
    SpzNodeAddReClass(target, "secondary");
}

// Sets the title of the dialog.
function DialogSetTitlebar(text) {
    let dialogTitlebar = document.getElementById('dialog').querySelectorAll('.titletext')[0];
    dialogTitlebar.innerHTML = text;
}

// Event handler that tries to select an object under the cursor for the current
// mouse event. This only selects a guard or pad. If an object is selected, return
// true, otherwise return false.
function HandleMaybeSelectItem(e) {
    if (!g_AllowDialog) {
        return false;
    }

    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return false;
    }

    const x = e.clientX;
    const y = e.clientY;

    let items = xml.elementsFromPoint(x, y);
    if (!items || items.length < 1) {
        return false;
    }

    for (let item of items) {
        let node = item;
        if (!item.classList.contains('svg-logical-item')) {
            node = item.closest('.svg-logical-item');
            if (!node || node.tagName == "svg") {
                continue;
            }
        }

        if (!EnsureNodeTypedef(node)) {
            continue;
        }

        if (node == g_SelectedItem) {
            return true;
        }

        if (node.dataset.display === 'Guard'
            || node.dataset.display === 'Pad')
        {
            SvgUnselectItems();
            DialogSelectItem(node);
            g_SelectedItem = node;
            node.classList.add("selected");
            return true;
        }
    }

    return false;
}

// entry for click in annotation node.
function HandleMaybeClickAnnotation(e) {
    if (g_AnnotateState == 0) {
        return false;
    } else if (g_AnnotateState == 10 || g_AnnotateState == 11) {
        return HandleAnnotateClickGuardAimLimit(e);
    } else if (g_AnnotateState == 20 || g_AnnotateState == 21) {
        return HandleAnnotateClickCircle(e);
    } else if (g_AnnotateState == 30) {
        return HandleAnnotateClickNoise(e);
    } else if (g_AnnotateState == 99) {
        return HandleAnnotateClickErase(e);
    } else {
        return false;
    }
}

// Helper event handler, drawing guard aim limit in annotation mode.
function HandleAnnotateClickGuardAimLimit(e) {

    let mouse = MouseToScaledCoord(e);

    if (g_AnnotateState == 10) {
        // currently ghost button cursor, this click event needs to add the origin point
        // and add ghost line

        g_AnnotatePrevPoint = mouse;

        g_AnnotateState = 11;
        return true;
    } else if (g_AnnotateState == 11) {
        // currently in ghost line mode, need to remove ghost line, remove origin point,
        // and place aim limit with correct rotation.

        let angle = Math.atan2(mouse.y - g_AnnotatePrevPoint.y, mouse.x - g_AnnotatePrevPoint.x);
        DrawGuardAimLimit({ x: g_AnnotatePrevPoint.x, y: g_AnnotatePrevPoint.y, angle: angle });

        ResetAnnotateState();
        return true;
    }
}

// Helper event handler, drawing a circle in annotation mode.
function HandleAnnotateClickCircle(e) {

    if (g_AnnotateState == 20) {
        // currently ghost button cursor, this click event needs to add the origin point
        // and add ghost line

        let mouse = MouseToScaledCoord(e);
        g_AnnotatePrevPoint = mouse;

        g_AnnotateState = 21;
        return true;
    } else if (g_AnnotateState == 21) {
        // currently in ghost line mode, need to remove ghost line, remove origin point,
        // and place circle.

        let svgCoords = MouseToScaledCoord(e);

        let xside = svgCoords.x - g_AnnotatePrevPoint.x;
        let yside = svgCoords.y - g_AnnotatePrevPoint.y;
        let r = Math.sqrt(xside * xside + yside * yside);

        DrawCircle({ x: g_AnnotatePrevPoint.x, y: g_AnnotatePrevPoint.y, radius: r, fillColour: "rgba(66, 236, 245, 0.4)" });

        ResetAnnotateState();
        return true;
    }
            
    return false;
}

// Helper event handler, drawing noise circle in annotation mode.
function HandleAnnotateClickNoise(e) {
    if (g_AnnotateState == 30) {

        let svgCoords = MouseToScaledCoord(e);

        let selectNode = document.getElementById('annotate-noise-weapon');
        if (!selectNode) {
            return false;
        }

        let index = selectNode.selectedIndex;
        let node = selectNode.options[index];

        if (!node) {
            return false;
        }

        let hearing = parseInt(document.getElementById('annotate-noise-hearing').value);
        // decomp formula: (guard.hearing / 1000) * noise * 100.0f
        let scale = hearing / 10;
        if (!scale) {
            return false;
        }

        // draw min hearing distance circle.
        let rmin = parseInt(node.dataset.min);
        let step = parseInt(node.dataset.step);
        let rmax = parseInt(node.dataset.max);
        if (rmin < 1) {
            rmin = step;
        }
        // only draw one circle if min/step == max
        if (rmax > rmin) {
            DrawCircle({ x: svgCoords.x, y: svgCoords.y, radius: (scale * rmin), fillColour: "rgba(255, 43, 43, 0.2)" });
        }

        // draw max hearing distance circle
        DrawCircle({ x: svgCoords.x, y: svgCoords.y, radius: (scale * rmax), fillColour: "rgba(243, 255, 69, 0.3)" });

        ResetAnnotateState();
        return true;
    }

    return false;
}

// Helper event handler, to click on an annotation to remove it.
function HandleAnnotateClickErase(e) {

    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return false;
    }

    const x = e.clientX;
    const y = e.clientY;

    let items = xml.elementsFromPoint(x, y);
    if (!items || items.length < 1) {
        return false;
    }

    for (let item of items) {
        let node = item;
        if (!item.classList.contains('logical-annotation')) {
            node = item.closest('.logical-annotation');
            if (!node || node.tagName == "svg") {
                continue;
            }

            node.remove();
            ResetAnnotateState();

            return true;
        }
    }

    return false;
}

// Button event handler for annotation "aim limit" button.
function HandleButtonAimLimit(e) {
    if (g_UseMode != UseMode.Annotate) {
        return;
    }
    if (!g_ZoomerLoaded) {
        return;
    }
    if (g_AnnotateState != 0) {
        return;
    }

    g_AnnotateState = 10;
    UiAnnotateDisableOtherButtons(e.target.closest('[onclick]'));
}

// Button event handler for annotation "circle" button.
function HandleButtonCircle(e) {
    if (g_UseMode != UseMode.Annotate) {
        return;
    }
    if (!g_ZoomerLoaded) {
        return;
    }
    if (g_AnnotateState != 0) {
        return;
    }

    g_AnnotateState = 20;
    UiAnnotateDisableOtherButtons(e.target.closest('[onclick]'));
}

// Button event handler for annotation "erase 1" button.
function HandleButtonErase1(e) {
    if (g_UseMode != UseMode.Annotate) {
        return;
    }
    if (!g_ZoomerLoaded) {
        return;
    }
    if (g_AnnotateState != 0) {
        return;
    }

    g_AnnotateState = 99;
    UiAnnotateDisableOtherButtons(e.target.closest('[onclick]'));
}

// Button event handler for annotation "erase all" button.
function HandleButtonEraseAll() {
    if (g_UseMode != UseMode.Annotate) {
        return;
    }
    if (!g_ZoomerLoaded) {
        return;
    }
    if (g_AnnotateState != 0) {
        return;
    }

    SvgNodeEmpty('annotation-container');
}

// Button event handler for annotation "noise" button.
function HandleButtonNoise(e) {
    if (g_UseMode != UseMode.Annotate) {
        return;
    }
    if (!g_ZoomerLoaded) {
        return;
    }
    if (g_AnnotateState != 0) {
        return;
    }

    g_AnnotateState = 30;
    UiAnnotateDisableOtherButtons(e.target.closest('[onclick]'));
}

// Resets current g_AnnotateState, and clears any annotation ghost objects from the svg.
function ResetAnnotateState() {
    g_AnnotateState = 0;

    Array.from(document.getElementById('control-buttons-annotate').querySelectorAll('button')).forEach(x => {
        x.classList.remove("ann-state-disable");
    });
    Array.from(document.getElementById('control-buttons-annotate').querySelectorAll('input[type="button"]')).forEach(x => {
        x.classList.remove("ann-state-disable");
    });

    RemoveAllAnnotationStateNodes();
}

// Applies styling to the annotation buttons when one of them is clicked
function UiAnnotateDisableOtherButtons(self) {
    Array.from(document.getElementById('control-buttons-annotate').querySelectorAll('button')).forEach(x => {
        if (x == self) {
            return;
        }
        x.classList.add("ann-state-disable");
    });
    Array.from(document.getElementById('control-buttons-annotate').querySelectorAll('input[type="button"]')).forEach(x => {
        if (x == self) {
            return;
        }
        x.classList.add("ann-state-disable");
    });
}

// Pans to g_SelectedItem and adds "selected" class
function SpzSelectAndPanToSelectedItem() {
    SpzPanToItem(g_SelectedItem);
    SpzNodeAddReClass(g_SelectedItem, "selected");
}

// Pans to element in the svg. Object is required to have "sc-x" and "sc-z" data attributes.
function SpzPanToItem(node) {
    if (!node) {
        return;
    }

    if (!g_ZoomerLoaded) {
        return;
    }

    // init + error checking, don't actually need.
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    if (!node.dataset.scX || !node.dataset.scY) {
        return;
    }

    let spz = svgPanZoom(document.getElementById('mmap'));

    let zoom = spz.getZoom();

    // try to adjust for rounding error
    if ((g_MinZoomToPan - zoom) > 0.01 ) {
        spz.zoom(g_MinZoomToPan);
    }

    let sizes = spz.getSizes();
    var areaWidth = parseInt(getComputedStyle(document.getElementById('mmap')).width);
    var areaHeight = parseInt(getComputedStyle(document.getElementById('mmap')).height);
    let x = (areaWidth / 2) - parseInt(node.dataset.scX) * sizes.realZoom;
    let y = (areaHeight / 2) - parseInt(node.dataset.scZ) * sizes.realZoom;
    spz.pan({ x: x, y: y });
}

// Removes a class then adds it again after a short delay.
function SpzNodeAddReClass(node, classname) {
    node.classList.remove(classname);
    // re-trigger border animation
    window.setTimeout(function () { node.classList.add(classname); }, 20);
}

// Clears "selected" and "secondary" items.
function SvgUnselectItems() {
    SvgUnselectPrimary();
    SvgUnselectSecondary();
}

// Clears "selected" item.
function SvgUnselectPrimary() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let svgSelected = svg.querySelectorAll('.selected');
    for (const node of svgSelected) {
        node.classList.remove("selected");
    }
}

// Clears "secondary" selected item.
function SvgUnselectSecondary() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let svgSecondary = svg.querySelectorAll('.secondary');
    for (const node of svgSecondary) {
        node.classList.remove("secondary");
    }
}

// For a given object, loads it into the dialog as the selected item.
// All prior content is cleared.
// If argument has "dataset.xAiInit" or "dataset.xAiId", those are shown.
function DialogSelectItem(node) {
    if (!g_AllowDialog) {
        return;
    }

    if (!g_ZoomerLoaded) {
        return;
    }

    if (!EnsureNodeTypedef(node)) {
        return;
    }

    EnsureDialog();

    let dnode = document.getElementById('dialog-selected-item');
    dnode.innerHTML = "";

    let mouseover_log = GetNodeLogText(node);

    let infoNode = document.createElement('div');

    let baseTextNode = document.createElement('div');
    baseTextNode.innerHTML = mouseover_log;
    infoNode.appendChild(baseTextNode);

    let panToNodeParent = document.createElement('div');
    let panToNode = document.createElement('input');
    panToNode.setAttribute("type", "button");
    panToNode.value = "goto";
    panToNode.onclick = SpzSelectAndPanToSelectedItem;
    panToNodeParent.appendChild(panToNode);
    infoNode.appendChild(panToNodeParent);

    if (node.dataset.xAiInit) {
        DialogAppendSeperator(infoNode);

        let aiInitNode = document.createElement('input');
        aiInitNode.setAttribute("type", "button");
        let id = parseInt(node.dataset.xAiInit);
        aiInitNode.value = `ai init: 0x${id.toString(16)}`;
        aiInitNode.dataset.xAiId = parseInt(id);
        aiInitNode.onclick = HandleDialogShowAiScript;
        infoNode.appendChild(aiInitNode);
    }

    if (node.dataset.xAiId) {
        DialogAppendSeperator(infoNode);

        let ids = SplitAttrTextArr(node.dataset.xAiId);

        for (let id of ids) {
            id = parseInt(id);
            let aiRefNode = document.createElement('input');
            aiRefNode.setAttribute("type", "button");
            aiRefNode.value = `ai ref: 0x${id.toString(16)}`;
            aiRefNode.dataset.xAiId = parseInt(id);
            aiRefNode.onclick = HandleDialogShowAiScript;
            infoNode.appendChild(aiRefNode);
        }

    }

    dnode.dataset.selectedItemId = node.id;
    dnode.appendChild(infoNode);

    if (node.dataset.displayId) {
        let intId = parseInt(node.dataset.displayId);
        DialogSetTitlebar(`${node.dataset.display}: id=${intId} (0x${intId.toString(16)})`);
    } else {
        DialogSetTitlebar(`${node.dataset.display}`);
    }

    DialogClearAi();

    g_DialogBox.showDialog();
}

// Eventhandler for an ai script button added to the dialog at runtime.
// Will load the ai script in the bottom portion of the dialog.
function HandleDialogShowAiScript(e) {
    if (!g_AllowDialog) {
        return;
    }

    var node = this;

    let id = parseInt(node.dataset.xAiId);
    DialogLoadAiScriptContent(id, true);
}

// Adds a separtor to the dialog content.
function DialogAppendSeperator(parent) {
    let sep = document.createElement('div');
    sep.classList.add('dialog-content-seperator');
    parent.appendChild(sep);
}

// Guards can be referenced by multiple ai scripts. The ids
// are stored in an attribute as "[1,2,3,4,...]". This parses
// the text and returns an array of the ids.
function SplitAttrTextArr(text) {
    if (!text || text.length < 2) {
        return [];
    }

    let wo = text.substr(1, text.length - 2);
    return wo.split(',').map(x => parseInt(x));
}

// Clears selected item section on the dialog.
function DialogClearSelected() {
    let dnode = document.getElementById('dialog-selected-item');
    dnode.dataset.selectedItemId = "";
    dnode.innerHTML = "";
}

// Clears ai section on the dialog.
function DialogClearAi() {
    let dnode = document.getElementById('dialog-ailist');
    dnode.innerHTML = "";
}

// Rebuilds the vertical bounds slider based on g_StageBounds.
function RebuildSlider() {

    let slider = document.getElementById('slider');
    if (!g_SliderInit) {
        g_SliderInit = true;
        noUiSlider.create(slider, {
            start: [g_StageBounds.scaled.min.y, g_StageBounds.scaled.max.y],
            connect: true,
            range: {
                'min': g_StageBounds.scaled.min.y,
                'max': g_StageBounds.scaled.max.y
            }
        });

        slider.noUiSlider.on('change.one', FilterVertivalSliderHandler);
    } else {
        let newOptions = {
            start: [g_StageBounds.scaled.min.y, g_StageBounds.scaled.max.y],
            range: {
                'min': g_StageBounds.scaled.min.y,
                'max': g_StageBounds.scaled.max.y
            }
        };
        slider.noUiSlider.updateOptions(
            newOptions, // Object
            true // Boolean 'fireSetEvent'
        );
    }

    document.getElementById('slider-left-y').value = g_StageBounds.scaled.min.y;
    document.getElementById('slider-right-y').value = g_StageBounds.scaled.max.y;
}

// Event handler when the user clicks the button to apply the slider bounds.
function ApplyManualBounds() {
    let slider_min = parseFloat(document.getElementById('slider-left-y').value);
    let slider_max = parseFloat(document.getElementById('slider-right-y').value);

    if (!isFinite(slider_min) || !isFinite(slider_max)) {
        return;
    }

    console.log('filter: min=' + slider_min + ', max=' + slider_max);

    FilterYCommon(slider_min, slider_max);

    document.getElementById('slider-left-y').value = slider_min;
    document.getElementById('slider-right-y').value = slider_max;
}

// Slider event handler.
function FilterVertivalSliderHandler(values, handle, unencoded, tap, positions, noUiSlider) {

    let slider_min = parseFloat(values[0]);
    let slider_max = parseFloat(values[1]);

    console.log('filter: min=' + slider_min + ', max=' + slider_max);

    FilterYCommon(slider_min, slider_max);

    document.getElementById('slider-left-y').value = slider_min;
    document.getElementById('slider-right-y').value = slider_max;
}

// Common vertical bounds filtering method.
function FilterYCommon(lower, upper) {
    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return;
    }

    // undo previous vertical filtering
    Array.from(xml.querySelectorAll('[data-sc-y]')).forEach(x => x.classList.remove('vertical-bound-filter'));
    Array.from(xml.querySelectorAll('[data-s-min-y]')).forEach(x => x.classList.remove('vertical-bound-filter'));

    Array.from(xml.querySelectorAll('[data-sc-y]:not(svg)')).filter(x => Number(x.dataset.scY) < lower || Number(x.dataset.scY) > upper).forEach(x => x.classList.add('vertical-bound-filter'));
    Array.from(xml.querySelectorAll('[data-s-min-y]:not(svg)')).filter(x => Number(x.dataset.sMinY) < lower || Number(x.dataset.sMaxY) > upper).forEach(x => x.classList.add('vertical-bound-filter'));
}

// Primary mouse move event handler.
function HandleMouseMove(e) {
    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return;
    }

    // always show mouse position.
    LogMouseCoord(e);

    // Draw cursor ghost dot if required
    UiSetFakeMarkerToMouseEvent(e);

    // Draw fake line to cursor if required.
    let minfo = UiDrawFakeLineToMouseEvent(e);

    // If this is ruler mode, with ghost line, show the mouse distance and return.
    if (g_UseMode == UseMode.Ruler) {

        if (g_RulerDropMode == 2) {
            RulerUpdateStatusBar(g_TotalDistance, g_TotalDistance + minfo.r);
        }

        return;
    }

    // If this is annotate mode, with ghost line, show radius distance and return.
    if (g_UseMode == UseMode.Annotate && g_AnnotateState == 21) {
        AnnotateUpdateStatusBar(minfo.r);

        return;
    }

    if (g_UseMode == UseMode.Move
        || g_UseMode == UseMode.Annotate) {
        // continue and log the item under the mouse;
    } else {
        return;
    }

    const x = e.clientX;
    const y = e.clientY;

    let items = xml.elementsFromPoint(x, y);
    if (!items || items.length < 1) {
        return;
    }

    let logStatusBar = false;

    //console.log('HandleMouseMove');

    for (let item of items) {
        let node = item;
        if (!item.classList.contains('svg-logical-item')) {
            node = item.closest('.svg-logical-item');
            if (!node || node.tagName == "svg") {
                continue;
            }
        }

        if (!logStatusBar) {
            logStatusBar = true;
            LogMouseOver(node);
            break;
        }
        //console.log('mousemove: ' + node.id);
    }
}

// Updates the status bar to show the map mouse coord.
function LogMouseCoord(e) {

    if (!g_ZoomerLoaded) {
        return;
    }

    let x = 1.0;
    let y = 1.0;

    //console.log(`mouse: ${e.clientX}, ${e.clientY}`);

    let p = MouseToScaledCoord(e);
    x = p.x;
    y = p.y;

    x = FloatToString(x, 3);
    y = FloatToString(y, 3);

    let mouseover_log = `pos: ${x}, ${y}`;

    document.getElementById('status-mouse-coord').innerHTML = mouseover_log;
}

// For a given mouse event, returns the position on the svg map in scaled world coordinates.
function MouseToScaledCoord(e) {

    let svg = GetSvg();
    if (!svg) {
        return;
    }

    if (!g_SvgPoint) {
        return;
    }

    g_SvgPoint.x = e.clientX;
    g_SvgPoint.y = e.clientY;
    var svgpoint = g_SvgPoint.matrixTransform(svg.getScreenCTM().inverse());

    let spz = svgPanZoom(document.getElementById('mmap'));
    let pan = spz.getPan();

    let x = (svgpoint.x - pan.x) / spz.getSizes().realZoom;
    let y = (svgpoint.y - pan.y) / spz.getSizes().realZoom;

    return { "x": x, "y": y };
}

// The svg doesn't include the getools type information by default.
// This helper adds that additional information onto the node.
// used in GetNodeLogText
function EnsureNodeTypedef(node) {
    if (node.dataset.ig) {
        return false;
    }

    if (!node.dataset.typedef) {
        let nodetype = NodeIdStringToType(node.id);
        if (!nodetype) {
            node.dataset.ig = 1;
            return false;
        }
        node.dataset.typedef = nodetype['typemap']['type'];
        node.dataset.display = nodetype['typemap']['display'];

        if (node.dataset.display === 'Guard'
            || node.dataset.display === 'Pad')
        {
            node.dataset.displayId = nodetype["match"][1];
        }
    }

    return true;
}

// Updates the mouse over section of the status bar.
// This depends on g_UseMode.
function LogMouseOver(node) {

    if (!g_ZoomerLoaded) {
        return;
    }

    if (!EnsureNodeTypedef(node)) {
        return;
    }

    let mouseover_log = GetNodeLogText(node);

    if (g_UseMode == UseMode.Move) {
        document.getElementById('status-mouse-item').innerHTML = mouseover_log;
    } else if (g_UseMode == UseMode.Annotate) {
        document.getElementById('status-mouse-annotate').innerHTML = mouseover_log;
    }
}

// Calculate the text to show in the mouseover status bar section.
// Assumes EnsureNodeTypedef was called prior to this function.
function GetNodeLogText(node) {
    let mouseover_log = "";

    if (node.dataset.display === 'Patrol') {
        mouseover_log += `${node.dataset.display}: id=${node.dataset.patrolNodeId}`;
    } else if (node.dataset.displayId) {
        mouseover_log += `${node.dataset.display}: id=${node.dataset.displayId}`;
    } else {
        mouseover_log += `${node.dataset.display}: roomid=${node.dataset.roomId}`;
    }

    // scaled coord
    if (node.dataset.scX) {
        mouseover_log += ` (${node.dataset.scX}, ${node.dataset.scY}, ${node.dataset.scZ})`;
    }

    if (node.dataset.propName) {
        mouseover_log += ` ${node.dataset.propName}`;
    }

    if (node.dataset.display === 'Guard') {
        if (node.dataset.chrClone !== undefined) {
            mouseover_log += ", CLONE";
        }
        if (node.dataset.chrInvincible !== undefined) {
            mouseover_log += ", INVINCIBLE";
        }
        mouseover_log += `, hearing: ${node.dataset.chrHdist}`;
        mouseover_log += `, visibility: ${node.dataset.chrVdist}`;
    }

    return mouseover_log;
}

// convert svg node id to the "type container" map object.
function NodeIdStringToType(id) {
    for (let ctm of ClassTypeMap) {
        let match = id.match(ctm.re);
        if (match) {
            return {
                "typemap": ctm,
                "match": match
            };
        }
    }
    return null;
}

function PanZoomIn() {
    if (!g_ZoomerLoaded) {
        return;
    }
    let spz = svgPanZoom(document.getElementById('mmap'));
    spz.zoomIn();
}
function PanZoomReset() {
    if (!g_ZoomerLoaded) {
        return;
    }
    let spz = svgPanZoom(document.getElementById('mmap'));
    spz.resetZoom();
    spz.center();
}
function PanZoomOut() {
    if (!g_ZoomerLoaded) {
        return;
    }
    let spz = svgPanZoom(document.getElementById('mmap'));
    spz.zoomOut();
}

// Gets the svg object or null.
function GetSvg() {
    if (!g_ZoomerLoaded) {
        return null;
    }

    let xml = document.getElementById('mmap').contentDocument;
    if (!xml) {
        return null;
    }

    let svg = xml.getElementsByTagName('svg');
    if (!svg || svg.length != 1) {
        return null;
    }

    return svg[0];
}

// Changes global app mode.
function SetControlMode(mode) {

    if (!(typeof mode === 'symbol')) {
        return;
    }

    g_UseMode = mode;

    UpdateUiForControlMode();
}

// Updates ui buttons for global app mode (g_UseMode).
function UpdateUiForControlMode() {
    let btn = null;

    if (g_UseMode == UseMode.Ruler) {
        document.getElementById('control-buttons-ruler').style.display = '';
        document.getElementById('control-buttons-move').style.display = 'none';
        document.getElementById('control-buttons-annotate').style.display = 'none';

        document.getElementById('status-ruler-dist').style.display = '';
        document.getElementById('status-mouse-item').style.display = 'none';
        document.getElementById('status-mouse-annotate').style.display = 'none';

        btn = document.getElementById('mode-control-ruler').getElementsByTagName('button')[0];
        btn.style.opacity = 0.3;
        btn.disabled = true;

        btn = document.getElementById('mode-control-move').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

        btn = document.getElementById('mode-control-annotate').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

    } else if (g_UseMode == UseMode.Move) {
        document.getElementById('control-buttons-ruler').style.display = 'none';
        document.getElementById('control-buttons-move').style.display = '';
        document.getElementById('control-buttons-annotate').style.display = 'none';

        document.getElementById('status-ruler-dist').style.display = 'none';
        document.getElementById('status-mouse-item').style.display = '';
        document.getElementById('status-mouse-annotate').style.display = 'none';

        btn = document.getElementById('mode-control-ruler').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

        btn = document.getElementById('mode-control-move').getElementsByTagName('button')[0];
        btn.style.opacity = 0.3;
        btn.disabled = true;

        btn = document.getElementById('mode-control-annotate').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

    } else if (g_UseMode == UseMode.Annotate) {
        document.getElementById('control-buttons-ruler').style.display = 'none';
        document.getElementById('control-buttons-move').style.display = 'none';
        document.getElementById('control-buttons-annotate').style.display = '';

        document.getElementById('status-ruler-dist').style.display = 'none';
        document.getElementById('status-mouse-item').style.display = 'none';
        document.getElementById('status-mouse-annotate').style.display = '';

        btn = document.getElementById('mode-control-ruler').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

        btn = document.getElementById('mode-control-move').getElementsByTagName('button')[0];
        btn.style.opacity = 1;
        btn.disabled = false;

        btn = document.getElementById('mode-control-annotate').getElementsByTagName('button')[0];
        btn.style.opacity = 0.3;
        btn.disabled = true;
    } else {
        return;
    }

    HandleButtonStop();
    ResetAnnotateState();
}

// Main event handler for mouse click on svg.
function HandleSvgClick(e) {

    if (g_UseMode == UseMode.Move) {
        if (g_AllowDialog) {
            HandleMaybeSelectItem(e);
        }
    } else if (g_UseMode == UseMode.Ruler) {
        HandleRulerClick(e);
    } else if (g_UseMode == UseMode.Annotate) {
        if (!HandleMaybeClickAnnotation(e)) {
            if (g_AllowDialog) {
                HandleMaybeSelectItem(e);
            }
        }
    }
}

// Helper mouse click event handler, for ruler mode.
function HandleRulerClick(e) {
    if (!g_ZoomerLoaded) {
        return;
    }

    if (g_RulerDropMode == 0) {
        return;
    }

    //console.log(`click at: ${e.x}, ${e.y}`);

    let coords = UiAppendDotAtMouseEvent(e);

    if (g_RulerDropMode == 2) {

        let minfo = UiAppendLineToMouseEvent(e, { parentId: 'user-ruler-container', lineClass: 'user-line', prev: g_RulerPrevPoint });

        g_TotalDistance += minfo.r;

        RulerUpdateStatusBar(g_TotalDistance, null);
    }

    g_RulerPrevPoint.x = coords.translated.x;
    g_RulerPrevPoint.y = coords.translated.y;

    if (g_RulerDropMode == 1) {
        g_RulerDropMode = 2;

        //document.getElementById('fake-line').style.display = '';
    }
}

// Event handler for ruler "start" button.
function HandleButtonStart() {
    if (!g_ZoomerLoaded) {
        return;
    }

    if (g_RulerDropMode == 0) {

        let spz = svgPanZoom(document.getElementById('mmap'));
        spz.disablePan();
        spz.disableZoom();

        RemoveAllUserRulerNodes();
        g_LastRulerDropMode = 0;

        g_TotalDistance = 0;
        g_RulerDropMode = 1;

        RulerUpdateStatusBar(0, null);

        UiUpdateRulerControlButtons();
    }
}

// Event handler for ruler "stop" button.
function HandleButtonStop() {
    if (!g_ZoomerLoaded) {
        return;
    }

    let spz = svgPanZoom(document.getElementById('mmap'));
    spz.enablePan();
    spz.enableZoom();

    g_LastRulerDropMode = g_RulerDropMode;
    if (g_RulerDropMode > 0) {
        g_RulerDropMode = 0;

        parent = EnsureSvgRootContainer('user-ruler-container');
        if (!parent) {
            return null;
        }

        UiHideFakeMarkers({ parent: parent });
    }

    RulerUpdateStatusBar(g_TotalDistance, null);

    UiUpdateRulerControlButtons();
}

// Event handler for ruler "continue" button.
function HandleButtonContinue() {
    if (!g_ZoomerLoaded) {
        return;
    }

    if (g_LastRulerDropMode == 2) {
        g_RulerDropMode = 2;
    } else if (g_LastRulerDropMode == 1) {
        g_RulerDropMode = 1;
    } else {
        return;
    }

    let spz = svgPanZoom(document.getElementById('mmap'));
    spz.disablePan();
    spz.disableZoom();

    UiUpdateRulerControlButtons();
}

// Event handler for ruler "clear" button.
function HandleButtonClear() {
    if (!g_ZoomerLoaded) {
        return;
    }

    g_TotalDistance = 0;
    g_RulerDropMode = 0;
    g_LastRulerDropMode = 0;
    RemoveAllUserRulerNodes();
    UiUpdateRulerControlButtons();

    parent = EnsureSvgRootContainer('user-ruler-container');
    if (!parent) {
        return null;
    }

    UiHideFakeMarkers({ parent: parent });
    RulerUpdateStatusBar(0, null);
}

// Updates styling and disabled attribute for ruler buttons.
function UiUpdateRulerControlButtons() {

    let bstart = document.getElementById('ruler-btn-start');
    let bstop = document.getElementById('ruler-btn-stop');
    let bcontinue = document.getElementById('ruler-btn-continue');
    let bclear = document.getElementById('ruler-btn-clear');

    if (g_RulerDropMode == 0) {
        // not started
        bstart.removeAttribute("disabled");
        bstart.style.opacity = 1;
        bstop.setAttribute("disabled", "disabled");
        bstop.style.opacity = 0.3;
    } else {
        // started
        bstart.setAttribute("disabled", "disabled");
        bstart.style.opacity = 0.3;
        bstop.removeAttribute("disabled");
        bstop.style.opacity = 1;
    }

    if (g_LastRulerDropMode == 0) {
        // no previous points
        bcontinue.setAttribute("disabled", "disabled");
        bcontinue.style.opacity = 0.3;
    } else {
        bcontinue.removeAttribute("disabled");
        bcontinue.style.opacity = 1;
    }

    bclear.removeAttribute("disabled");
    bclear.style.opacity = 1;
}

// Hides ghost markers, depending on opt.parent.
function UiHideFakeMarkers(opt) {
    RemoveFakeDot(opt);
    RemoveFakeLine(opt);
}

// Ensures svg dom node with id exists, or creates it as <g> element.
function EnsureSvgRootContainer(id) {
    let svg = GetSvg();
    if (!svg) {
        return null;
    }

    let svgSpz = svg.getElementsByClassName('svg-pan-zoom_viewport');
    if (!svgSpz || svgSpz.length < 1) {
        return null;
    }

    svgSpz = svgSpz[0];

    let items = svgSpz.querySelectorAll("#" + id);
    let item = null;

    if (!items || items.length < 1) {
        item = document.createElementNS(svgns, 'g');
        item.id = id;
        svgSpz.appendChild(item);
    } else {
        item = items[0];
    }

    return item;
}

// Ensures ghost dot object exists in svg, under opt.parent.
function EnsureFakeDot(opt) {
    let parent = opt.parent;
    let dotclass = opt.dotclass;

    let fakeDot = parent.getElementsByClassName('the-fake-dot');

    if (!fakeDot || fakeDot.length < 1) {
        fakeDot = document.createElementNS(svgns, 'circle');
        fakeDot.setAttributeNS(null, 'r', g_DotDiameter);
        fakeDot.classList.add('the-fake-dot');
        fakeDot.classList.add(dotclass);
        fakeDot.setAttributeNS(null, 'style', 'fill: #00ffc3; stroke: #4580ff; stroke-width: 2px; opacity: 0.5;');
        parent.appendChild(fakeDot);
    } else {
        fakeDot = fakeDot[0];
    }

    return fakeDot;
}

// Removes ghost dot object from svg, under opt.parent.
function RemoveFakeDot(opt) {
    let parent = opt.parent;

    let fakeDot = parent.getElementsByClassName('the-fake-dot');

    if (!fakeDot || fakeDot.length < 1) {
        return;
    } else {
        fakeDot = fakeDot[0];
        fakeDot.remove();
    }
}

// Ensures ghost line object exists in svg, under opt.parent.
function EnsureFakeLine(opt) {
    let parent = opt.parent;
    let lineClass = opt.lineClass;

    let fakeLine = parent.getElementsByClassName('the-fake-line');

    if (!fakeLine || fakeLine.length < 1) {
        fakeLine = document.createElementNS(svgns, 'line');
        fakeLine.setAttributeNS(null, 'x2', 0);
        fakeLine.setAttributeNS(null, 'y2', 0);
        fakeLine.setAttributeNS(null, 'stroke-dasharray', g_RulerLineDash);
        fakeLine.classList.add('the-fake-line');
        fakeLine.classList.add(lineClass);
        fakeLine.setAttributeNS(null, 'style', `stroke: #000000; stroke-width: ${g_RulerLineThickness}; opacity: 0.5;`);
        parent.appendChild(fakeLine);
    } else {
        fakeLine = fakeLine[0];
    }

    fakeLine.setAttributeNS(null, 'x1', opt.prev.x);
    fakeLine.setAttributeNS(null, 'y1', opt.prev.y);

    return fakeLine;
}

// Removes ghost line object from svg, under opt.parent.
function RemoveFakeLine(opt) {
    let parent = opt.parent;

    let fakeLine = parent.getElementsByClassName('the-fake-line');

    if (!fakeLine || fakeLine.length < 1) {
        return;
    } else {
        fakeLine = fakeLine[0];
        fakeLine.remove();
    }
}

// Draws a dot on the svg at the (screen coord) mouse event.
function UiAppendDotAtMouseEvent(e) {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let userRulerContainer = EnsureSvgRootContainer('user-ruler-container');

    let svgCoords = MouseToScaledCoord(e);

    let circle = document.createElementNS(svgns, 'circle');
    circle.setAttributeNS(null, 'cx', svgCoords.x);
    circle.setAttributeNS(null, 'cy', svgCoords.y);
    circle.setAttributeNS(null, 'r', g_DotDiameter);
    circle.classList.add('user-line');
    circle.setAttributeNS(null, 'style', 'fill: #00ffc3; stroke: #4580ff; stroke-width: 2px;');
    userRulerContainer.appendChild(circle);

    return {
        "screen": {
            "x": e.clientX,
            "y": e.clientY
        },
        "translated": {
            "x": svgCoords.x,
            "y": svgCoords.y
        }
    }
}

// Draws a line on the svg at the (screen coord) mouse event.
// Added under opt.parent
function UiAppendLineToMouseEvent(e, opt) {
    let results = {
        "angle": 0,
        "r": 0
    };

    let parent = EnsureSvgRootContainer(opt.parentId);
    if (!parent) {
        return null;
    }

    let fakeLine = EnsureFakeLine({ parent: parent, lineClass: opt.lineClass, prev: opt.prev });
    if (!fakeLine) {
        return null;
    }

    //console.log("g_RulerPrevPoint at: " + g_RulerPrevPoint.x + ", " + g_RulerPrevPoint.y);

    let svgCoords = MouseToScaledCoord(e);

    let xside = svgCoords.x - g_RulerPrevPoint.x;
    let yside = svgCoords.y - g_RulerPrevPoint.y;
    let r = Math.sqrt(xside * xside + yside * yside);

    let line = document.createElementNS(svgns, 'line');
    line.setAttributeNS(null, 'x1', g_RulerPrevPoint.x);
    line.setAttributeNS(null, 'y1', g_RulerPrevPoint.y);
    line.setAttributeNS(null, 'x2', svgCoords.x);
    line.setAttributeNS(null, 'y2', svgCoords.y);
    line.setAttributeNS(null, 'stroke-dasharray', g_RulerLineDash);
    line.classList.add(opt.lineClass);
    line.setAttributeNS(null, 'style', `stroke: #000000; stroke-width: ${g_RulerLineThickness}; `);
    parent.appendChild(line);

    results.r = r;
    return results;
}

// Moves the ghost dot to the (screen coord) mouse event.
function UiSetFakeMarkerToMouseEvent(e) {

    let parent = '';
    let fakeDot = null;

    if (g_UseMode == UseMode.Ruler) {
        if (g_RulerDropMode == 0) {
            // nothing to do.
            return;
        }

        parent = EnsureSvgRootContainer('user-ruler-container');
        if (!parent) {
            return null;
        }

        fakeDot = EnsureFakeDot({ parent: parent, dotclass: 'user-line' });
        if (!fakeDot) {
            return null;
        }
    } else if (g_UseMode == UseMode.Annotate) {

        if (g_AnnotateState == 10
            || g_AnnotateState == 20
            || g_AnnotateState == 30) {
            // continue, draw ghost dot on cursor
        } else {
            return;
        }

        parent = EnsureSvgRootContainer('annotation-container');
        if (!parent) {
            return null;
        }

        fakeDot = EnsureFakeDot({ parent: parent, dotclass: 'ann-state' });
        if (!fakeDot) {
            return null;
        }
    } else {
        return;
    }

    let svgCoords = MouseToScaledCoord(e);

    fakeDot.setAttributeNS(null, 'cx', svgCoords.x);
    fakeDot.setAttributeNS(null, 'cy', svgCoords.y);
}

// Draws/rotates ghost line to the screen coord mouse event.
function UiDrawFakeLineToMouseEvent(e) {
    let results = {
        "angle": 0,
        "r": 0
    };

    let parent = null;
    let fakeLine = null;
    let prev = null;

    if (g_UseMode == UseMode.Ruler
        && g_RulerDropMode == 2)
    {
        parent = EnsureSvgRootContainer('user-ruler-container');
        if (!parent) {
            return null;
        }

        prev = g_RulerPrevPoint;

        fakeLine = EnsureFakeLine({ parent: parent, lineClass: 'user-line', prev: prev });
        if (!fakeLine) {
            return null;
        }

    } else if (g_UseMode == UseMode.Annotate) {

        if (g_AnnotateState == 11
            || g_AnnotateState == 21) {
            // continue, draw line to cursor
        } else {
            return;
        }

        parent = EnsureSvgRootContainer('annotation-container');
        if (!parent) {
            return null;
        }

        prev = g_AnnotatePrevPoint;

        fakeLine = EnsureFakeLine({ parent: parent, lineClass: 'ann-state', prev: prev });
        if (!fakeLine) {
            return null;
        }
    } else {
        return null;
    }


    let svgCoords = MouseToScaledCoord(e);

    let xside = svgCoords.x - prev.x;
    let yside = svgCoords.y - prev.y;
    let r = Math.sqrt(xside * xside + yside * yside);

    //let angle = Math.atan2(yside, xside);

    fakeLine.setAttributeNS(null, 'x2', svgCoords.x);
    fakeLine.setAttributeNS(null, 'y2', svgCoords.y);

    results.r = r;
    return results;
}

// Deletes all contents under the svg node id.
function SvgNodeEmpty(id) {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let node = EnsureSvgRootContainer(id);
    node.innerHTML = "";
}

// Deletes all ruler content.
function RemoveAllUserRulerNodes() {
    SvgNodeEmpty('user-ruler-container');
}

// Deletes all annotation content.
function RemoveAllAnnotationStateNodes() {
    let svg = GetSvg();
    if (!svg) {
        return;
    }

    let userRulerContainer = EnsureSvgRootContainer('annotation-container');

    let nodes = userRulerContainer.getElementsByClassName('ann-state');
    while (nodes.length > 0) {
        for (const n of nodes) {
            n.remove();
        }
        nodes = userRulerContainer.getElementsByClassName('ann-state');
    }
}

// Convert a float to string, with the specified number of digits.
function FloatToString(f, digits) {
    let str = "" + f;
    if (!str) {
        return "";
    }
    if (digits < 1) {
        return str;
    }
    let dec = str.indexOf(".");
    if (dec < 1) {
        return str;
    }
    let len = str.length;
    if (dec + 1 + digits >= len) {
        return str;
    }
    return str.substr(0, dec + 1 + digits);
}

// Update the status bar in the standard way for ruler mode.
function RulerUpdateStatusBar(dist, hoverDist) {
    let html = '';
    if (Number.isFinite(dist)) {
        html += `dist: ${FloatToString(dist, 3)}`;
    }
    if (Number.isFinite(hoverDist)) {
        html += ` (to ${FloatToString(hoverDist, 3)})`;
    }
    document.getElementById('status-ruler-dist').innerHTML = html;
}

// Update the status bar in the standard way for annotation mode.
function AnnotateUpdateStatusBar(dist) {
    let html = '';
    if (Number.isFinite(dist)) {
        html += `radius: ${FloatToString(dist, 3)}`;
    }
    document.getElementById('status-mouse-annotate').innerHTML = html;
}

//

function BuildPieSlice(settings) {
    /*** https://www.codedrome.com/drawing-arcs-pie-slices-with-svg/ */
    let d = "";

    const firstCircumferenceX = settings.centreX + settings.radius * Math.cos(settings.startAngleRadians);
    const firstCircumferenceY = settings.centreY + settings.radius * Math.sin(settings.startAngleRadians);
    const secondCircumferenceX = settings.centreX + settings.radius * Math.cos(settings.startAngleRadians + settings.sweepAngleRadians);
    const secondCircumferenceY = settings.centreY + settings.radius * Math.sin(settings.startAngleRadians + settings.sweepAngleRadians);

    // move to centre
    d += "M" + settings.centreX + "," + settings.centreY + " ";
    // line to first edge
    d += "L" + firstCircumferenceX + "," + firstCircumferenceY + " ";
    // arc
    // Radius X, Radius Y, X Axis Rotation, Large Arc Flag, Sweep Flag, End X, End Y
    d += "A" + settings.radius + "," + settings.radius + " 0 0,1 " + secondCircumferenceX + "," + secondCircumferenceY + " ";
    // close path
    d += "Z";

    const arc = document.createElementNS(svgns, "path");

    arc.setAttributeNS(null, "d", d);
    arc.setAttributeNS(null, "fill", settings.fillColour);
    arc.setAttributeNS(null, "style", "stroke: #000000; stroke-width: 4px;");

    return arc;
}

// Draw guard aim limit as svg objects.
function DrawGuardAimLimit(settings) {

    const a1 = Math.PI * (180 - 167.5) / 180;
    const a2 = Math.PI * (180 - 83.5) / 180;
    const a3 = Math.PI * (180 - 42.0) / 180;
    const a4 = Math.PI * (180 - 21.0) / 180;
    const a5 = Math.PI * (180 - 12.5) / 180;

    let parent = EnsureSvgRootContainer('annotation-container');
    if (!parent) {
        return null;
    }

    let x = settings.x;
    let y = settings.y;

    const gnode = document.createElementNS(svgns, "g");
    gnode.classList.add("logical-annotation");

    let deg = settings.angle * 180 / Math.PI;

    gnode.setAttributeNS(null, "style", `transform-origin: ${x}px ${y}px; transform: rotate(${deg}deg);`);

    gnode.dataset.scX = x;
    gnode.dataset.scY = y;

    let rad = 0;
    let arc = null;

    rad = a1;
    arc = BuildPieSlice({ centreX: x, centreY: y, radius: 2400, startAngleRadians: -rad / 2, sweepAngleRadians: rad, fillColour: "rgba(255, 255, 0, 0.3)" });
    gnode.appendChild(arc);

    rad = a2;
    arc = BuildPieSlice({ centreX: x, centreY: y, radius: 1600, startAngleRadians: -rad / 2, sweepAngleRadians: rad, fillColour: "rgba(255, 200, 0, 0.3)" });
    gnode.appendChild(arc);

    rad = a3;
    arc = BuildPieSlice({ centreX: x, centreY: y, radius: 800, startAngleRadians: -rad / 2, sweepAngleRadians: rad, fillColour: "rgba(255, 165, 0, 0.3)" });
    gnode.appendChild(arc);

    rad = a4;
    arc = BuildPieSlice({ centreX: x, centreY: y, radius: 400, startAngleRadians: -rad / 2, sweepAngleRadians: rad, fillColour: "rgba(255, 115, 0, 0.3)" });
    gnode.appendChild(arc);

    rad = a5;
    arc = BuildPieSlice({ centreX: x, centreY: y, radius: 200, startAngleRadians: -rad / 2, sweepAngleRadians: rad, fillColour: "rgba(255, 75, 0, 0.3)" });
    gnode.appendChild(arc);

    parent.appendChild(gnode);
}

// Draw circle as svg object.
function DrawCircle(opt) {
    let parent = EnsureSvgRootContainer('annotation-container');
    if (!parent) {
        return null;
    }

    const gnode = document.createElementNS(svgns, "g");
    gnode.classList.add("logical-annotation");

    const circ = document.createElementNS(svgns, "circle");
    circ.setAttributeNS(null, "cx", opt.x);
    circ.setAttributeNS(null, "cy", opt.y);
    circ.setAttributeNS(null, "r", opt.radius);
    circ.setAttributeNS(null, "fill", opt.fillColour);
    circ.setAttributeNS(null, "style", "stroke: #000000; stroke-width: 4px;");

    gnode.appendChild(circ);
    parent.appendChild(gnode);
}
