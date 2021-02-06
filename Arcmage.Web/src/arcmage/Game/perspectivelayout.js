/* Region: Coordinate systems */

/* 
    There are three coordinate systems in play to achieve the perspective look and feel:
    
    The battlefield game coordinate system
    ======================================
    - It is the independent fixed coordinate system of the game, and it defines the battlefield as a 1920 (width) by 1200 (height) rectangle.
    - Cards are fixed in size and measure 106 (width) by 150 (height)
    - The top rectangle (top:0, left:0, bottom:600, right:1920) represents the opponent's battle field
    - The bottom rectangle (top:600, left:0, bottom:1200, right:1920) represents the player's battle field
    - It is used in the css layout and also used in the communication protocol

    The sreen coordinate system
    ===========================
    - The screen coordinate system is the available screen realestate given by the user's browser resolution,
    - Hence it is different for each connected user. It is used for input, and it's coordinate's are translated to the battlefield coordinate system

    The perspective coordinate system
    =================================
    - The perspective coordinate system is what is used to achieve the perspective layout. (css3d matrix transform)
    
    How it works:
    1. The sreen realestate is examined and a target perspective is defined:
       The target perspective (trapazoid) in screen coordinates is 
        - topleft  : 0 , 0 + 20% * screenwidth
        - topright : 0 , width - 20% * screenwidth
        - bottomleft: screenheight, 0
        - bottomright: screenheight, screenwidth
    2. The transformation matrices for screen => perspective and perspective => screen are calculated
    3. The transformation matrices for perspective => battlefield and battlefield => perspective are calculated
    
    These transformation matrices are used for setting up the css 3d rotation matrix and for user interaction.
    E.g. a card drag:
       - when a card starts dragging, jquery reports the screen coordinates of the bounding box of the dragged element
       - through the perspective => screen transformation we can calculate the center of the dragged element in perspective coordinates
       - when a card drag is being dragged, we do the same (calculate center of the dragged element), and thus know the drag-delta in perspective coordinates
       - then we use the battlefield => perspective tranformation to get the battlefield coorinates of the dragged element (which we set using css)
       - since all element's inside the #battleField are transformed to the perspective, the cards are dragged nicely.
    Hence 
*/

/* Result callback of the calculation of the screen => perspective and perspective => screen transformation matrices */
function screenTransformMatrixCallback(H, IH) {
	vue.screenTransformMatrix = H;
	vue.inverseScreenTransformMatrix = IH;
}

/* Result callback of the calculation of the battlefield => perspective and perspective => battlefield transformation matrices */
function transformMatrixCallback(el, H, IH){
	vue.transformMatrix = H;
	vue.inverseTransformMatrix = IH;
}

/* Help function degrees to rad */
function toRadians (angle) {
  return angle * (Math.PI / 180);
}

/* EndRegion: Coordinate systems */



/* Region: Perspective drag */
/* Extend draggable prototype with custom data, used for the perspective drag */
$.ui.draggable.prototype._uiHash = function() {
    return {
        helper: this.helper,
        position: this.position,
        originalPosition: this.originalPosition,
        offset: this.positionAbs,
        containment: this.containment,
    };
};

/* 
    When starting a pespective drag, we calculate the starting drag position in perspective coordinate system and store it.
    The drag helper is set to fixed, so it uses the coordinate's relative to the screen.
*/
function perspectiveDragStart (event, ui) {

    var dragData = $(ui.helper).data('dragdata');
    dragData.dropped = false;

    ui.helper.css({ position: 'fixed'});

	var offsetX = event.pageX;
	var offsetY = event.pageY;

	var offset = numeric.dot(vue.inverseTransformMatrix, [offsetX, offsetY, 0, 1]);
	offsetX = offset[0]/offset[3];
	offsetY = offset[1]/offset[3];
	
    vue.perspectiveDragOriginLeft = offsetX;
	vue.perspectiveDragOriginTop = offsetY;

    var width = $(this).width();
	var height = $(this).height();
	
	var left = ui.offset.left;
	var top = ui.offset.top;
	var right = ui.offset.left + width; 
	var bottom = ui.offset.top + height; 
	
	var topleft = numeric.dot(vue.inverseTransformMatrix, [left, top, 0, 1]);
	var topleftX = topleft[0]/topleft[3];
	var topleftY = topleft[1]/topleft[3];
	
	var bottomleft = numeric.dot(vue.inverseTransformMatrix, [left, bottom, 0, 1]);
	var bottomleftX = bottomleft[0]/bottomleft[3];
	var bottomleftY = bottomleft[1]/bottomleft[3];
	
	var topright = numeric.dot(vue.inverseTransformMatrix, [right, top, 0, 1]);
	var toprightX = topright[0]/topright[3];
	var toprightY = topright[1]/topright[3];
	
	var bottomright = numeric.dot(vue.inverseTransformMatrix, [right, bottom, 0, 1]);
	var bottomrightX = bottomright[0]/bottomright[3];
	var bottomrightY = bottomright[1]/bottomright[3];
	
	var minleft = (topleftX + bottomrightX)/2;
	var maxright = (toprightX + bottomleftX)/2;
	var centerX = Math.max(maxright,minleft);
	left = Math.round(centerX - 106/2.0);
	
	var minbottom = (topleftY + bottomrightY) /2;
	var maxbottom = (toprightY + bottomleftY) /2;
	var centerY = Math.max(maxbottom, minbottom);
	top = Math.round(centerY - 150/2.0);
	
	ui.position.left = left;
	ui.position.top = top;
	return true;
};
 
/* 
    When ending a pespective drag, the drag helper is no longer set to fixed.
*/
function perspectiveDragStop (event, ui) {
    var offsetX = event.pageX;
    var offsetY = event.pageY;

    var offset = numeric.dot(vue.inverseTransformMatrix, [offsetX, offsetY, 0, 1]);
    offsetX = offset[0] / offset[3];
    offsetY = offset[1] / offset[3];

    var leftDrag = offsetX - vue.perspectiveDragOriginLeft;
    var topDrag = offsetY - vue.perspectiveDragOriginTop;

    var left = ui.originalPosition.left + leftDrag;
    var top = ui.originalPosition.top + topDrag;
   
    ui.position.left = left;
    ui.position.top = top;

    var dragData = $(ui.helper).data('dragdata');
    dragData.top = top;
    dragData.left = left;
	ui.helper.css('position', '');
	return true;
};

/* 
    When draggin using pespective drag, we calculate the starting drag delta in perspective coordinate system,
    and apply it to the dragged element. We also update the dragdata to have to correct coordinates (used in a drop)
*/
function perspectiveDrag(event, ui) {

    var offsetX = event.pageX;
	var offsetY = event.pageY;

	var offset = numeric.dot(vue.inverseTransformMatrix, [offsetX, offsetY, 0, 1]);
	offsetX = offset[0]/offset[3];
	offsetY = offset[1]/offset[3];

    var leftDrag = offsetX - vue.perspectiveDragOriginLeft;
	var topDrag = offsetY - vue.perspectiveDragOriginTop;
	
	var left = ui.originalPosition.left + leftDrag;
	var top = ui.originalPosition.top + topDrag;

	ui.position.left = left;
	ui.position.top = top;
	
	var dragData = $(ui.helper).data('dragdata');
	dragData.top = top;
	dragData.left = left;
	
	return true;

};

Vue.directive('draggable', {
    bind: function () {
    },
    update: function (value) {

        $(this.el).data('dragdata', value);

        if (value.isDraggable) {
            $(this.el).draggable({
                connectToSortable: "#playerHand",
                stack: ".draggable-card",
                scroll: false,
                addClasses: false,
                start: perspectiveDragStart,
                drag: perspectiveDrag,
                stop: perspectiveDragStop,
            }).css('position', '');
        }
        else {
            $(this.el).draggable({
                disabled: true
            });
        }
    },
    unbind: function () {
    },
});
/* EndRegion: Perspective drag */

/* Region: Perspective Layout */

function resizeGame() {
    
	var height = Math.max(window.innerHeight, $(window).height());
	var width =  Math.max(window.innerWidth, $(window).width());
	var distance = width*0.2;
	
	$("#content").css({
	    width: width,
		height: height,
	});

	$("#content").width = width;
	$("#content").height = height;
 
	var originalPos = [];
	/*  left top */
	originalPos.push([0,0]);
	/* left bottom */
	originalPos.push([0,1200]);
	/* right top */
	originalPos.push([1920,0]);
	/* right bottom */
	originalPos.push([1920,1200]);
	
	
	
	var targetPos = [];
	/*  left top */
	targetPos.push([distance,0]);
	/* left bottom */
	targetPos.push([0,height]);
	/* right top */
	targetPos.push([width-distance,0]);
	/* right bottom */
	targetPos.push([width,height]);
	var element = $("#battleField")[0];
	
	applyTransform(element, originalPos, targetPos, transformMatrixCallback);
	
	var screenPos = [];
	/*  left top */
	screenPos.push([0,0]);
	/* left bottom */
	screenPos.push([0,height]);
	/* right top */
	screenPos.push([width,0]);
	/* right bottom */
	screenPos.push([width,height]);
	
	getTransformationMatrices(screenPos, targetPos, screenTransformMatrixCallback);

}

/* Region: Perspective Layout */
