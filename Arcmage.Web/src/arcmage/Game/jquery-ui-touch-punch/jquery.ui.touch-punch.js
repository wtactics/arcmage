/*!
 * jQuery UI Touch Punch 0.2.2
 *
 * Copyright 2011, Dave Furfero
 * Dual licensed under the MIT or GPL Version 2 licenses.
 *
 * Depends:
 *  jquery.ui.widget.js
 *  jquery.ui.mouse.js
 */
(function ($) {

  // Detect touch support
  $.support.touch = 'ontouchend' in document;

  // Ignore browsers without touch support
  if (!$.support.touch) {
    return;
  }

  var mouseProto = $.ui.mouse.prototype,
      _mouseInit = mouseProto._mouseInit,
      touchHandled;
  // N.G. Remark: support for long touch to right click
  var touchStartTimeStamp = 0;
  var touchEndTimeStamp   = 0;
  var prevTouchEndTimeStamp   = 0;
  var targetElement = null, prevTargetElement = null;

  /**
   * Simulate a mouse event based on a corresponding touch event
   * @param {Object} event A touch event
   * @param {String} simulatedType The corresponding mouse event
   */
  function simulateMouseEvent (event, simulatedType, button) {

    // Ignore multi-touch events
    if (event.originalEvent.touches.length > 1) {
      return;
    }

    event.preventDefault();

    var touch = event.originalEvent.changedTouches[0],
        simulatedEvent = document.createEvent('MouseEvents');
    
    // Initialize the simulated mouse event using the touch event's coordinates
    simulatedEvent.initMouseEvent(
      simulatedType,    // type
      true,             // bubbles                    
      true,             // cancelable                 
      window,           // view                       
      1,                // detail                     
      touch.screenX,    // screenX                    
      touch.screenY,    // screenY                    
      touch.clientX,    // clientX                    
      touch.clientY,    // clientY                    
      false,            // ctrlKey                    
      false,            // altKey                     
      false,            // shiftKey                   
      false,            // metaKey                    
      button,                // button                     
      null              // relatedTarget              
    );

    // Dispatch the simulated event to the target element
    event.target.dispatchEvent(simulatedEvent);
  }

  /**
   * Handle the jQuery UI widget's touchstart events
   * @param {Object} event The widget element's touchstart event
   */
  mouseProto._touchStart = function (event) {

    var self = this;

    // Ignore the event if another widget is already being handled
    if (touchHandled || !self._mouseCapture(event.originalEvent.changedTouches[0])) {
      return;
    }

    // Set the flag to prevent other widgets from inheriting the touch event
    touchHandled = true;

	// N.G. Remark: start of touch
	touchStartTimeStamp = event.timeStamp;

    // Track movement to determine if interaction was a click
    self._touchMoved = false;

    // Simulate the mouseover event
    simulateMouseEvent(event, 'mouseover', 0);

    // Simulate the mousemove event
    simulateMouseEvent(event, 'mousemove', 0);

    // Simulate the mousedown event
    simulateMouseEvent(event, 'mousedown', 0);
  };

  /**
   * Handle the jQuery UI widget's touchmove events
   * @param {Object} event The document's touchmove event
   */
  mouseProto._touchMove = function (event) {

    // Ignore event if not handled
    if (!touchHandled) {
      return;
    }

    // Interaction was not a click
    this._touchMoved = true;

    // Simulate the mousemove event
    simulateMouseEvent(event, 'mousemove', 0);
  };

  /**
   * Handle the jQuery UI widget's touchend events
   * @param {Object} event The document's touchend event
   */
  mouseProto._touchEnd = function (event) {

    // Ignore event if not handled
    if (!touchHandled) {
      return;
    }

    prevTouchEndTimeStamp = touchEndTimeStamp;
	touchEndTimeStamp = event.timeStamp;
	
	prevTargetElement = targetElement;
	targetElement = event.target;

    // Simulate the mouseup event
    simulateMouseEvent(event, 'mouseup', 0);

    // Simulate the mouseout event
    simulateMouseEvent(event, 'mouseout', 0);

    // If the touch interaction did not move, it should trigger a click
    if (!this._touchMoved) {

	  if(targetElement === prevTargetElement) {
		if (touchEndTimeStamp - prevTouchEndTimeStamp < 300) {
			// prevTouchEndTimeStamp = 0;
			// Simulate the click event
			simulateMouseEvent(event, 'dblclick', 0);
			return;
		  }

		  // N.G. Remark: check for long touch event
		  if (touchEndTimeStamp - touchStartTimeStamp > 800) {
			// prevTouchEndTimeStamp = 0;
			// Simulate the click event
			simulateMouseEvent(event, 'contextmenu', 2);
			return;
		  } 
	  }

	  // Simulate the click event
	  simulateMouseEvent(event, 'click', 0);

    }

    // Unset the flag to allow other widgets to inherit the touch event
    touchHandled = false;
  };

  /**
   * A duck punch of the $.ui.mouse _mouseInit method to support touch events.
   * This method extends the widget with bound touch event handlers that
   * translate touch events to mouse events and pass them to the widget's
   * original mouse event handling methods.
   */
  mouseProto._mouseInit = function () {
    
    var self = this;

    // Delegate the touch handlers to the widget's element
    self.element
      .bind('touchstart', $.proxy(self, '_touchStart'))
      .bind('touchmove', $.proxy(self, '_touchMove'))
      .bind('touchend', $.proxy(self, '_touchEnd'));

    // Call the original $.ui.mouse init method
    _mouseInit.call(self);
  };

})(jQuery);