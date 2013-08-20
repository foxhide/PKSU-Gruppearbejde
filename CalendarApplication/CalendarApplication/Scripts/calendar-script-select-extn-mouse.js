/* This script is one of two extensions for adding the 'click
 * and drag'-functions for room/time select.
 * This script is used for adding mouse-handlers, should be
 * used on normal computers, that use a mouse.
 */

/* Function for adding the selection functions to a room
   Cleans up mousemove,mouseup,mouseleave from the drag functions */
function addSelectionFunction(roomId) {
    $("#room_" + roomId)
    .unbind("mousemove")
    .unbind("mouseup")
    .unbind("mouseleave")
    .mousedown(function (e) {
        if (!selection) {
            s_top = e.pageY - $("#room_wrap_" + roomId).position().top;
            s_top = s_top > 660 ? 660 : s_top;
            s_top = round(s_top, ROUND_VALUE / 2); // Remember to include the pixel->minute convert (1px = 2min)
            s_bottom = s_top + 60;
            $(this).html($(this).html() + createSelection(roomId, s_top, 60));
            $("#clear_button").removeAttr("disabled");
            selection = true;
            addDragFunctions(roomId);
        }
        else if (!roomList[roomId].select) {
            $(this).html($(this).html() + createSelection(roomId, s_top, s_bottom - s_top));
            addDragFunctions(roomId);
        }
        roomList[roomId].select = true;
        checkRoom(roomId);
    });
}

/* Function for adding the dragging functions  */
function addDragFunctions(roomId) {
    // Add bottom drag handle
    $("#" + roomId + "_b_drag")
    .mousedown(function (e) {
        flagDown = true;
        $("#time_counter").html(getTime(s_bottom));
        $("#time_counter").show();
    })
    // Add top drag handle
    $("#" + roomId + "_t_drag")
    .mousedown(function (e) {
        flagUp = true;
        $("#time_counter").html(getTime(s_top));
        $("#time_counter").show();
    })
    // Add mouse movement and release for the room divs
    $("#room_" + roomId)
    .unbind("mousedown")
    .mousemove(function (e) {
        if (flagDown) {
            // Dragging down
            s_bottom = e.pageY - $("#room_wrap_" + roomId).position().top;
            if (s_bottom < s_top + 15) { s_bottom = s_top + 15; }
            else if (s_bottom > 720) { s_bottom = 720; }
            else { s_bottom = round(s_bottom, ROUND_VALUE / 2); }
            $(".selection").height(s_bottom - s_top);
            $("#time_counter").html(getTime(s_bottom));
        }
        else if (flagUp) {
            // Dragging up
            s_top = e.pageY - $("#room_wrap_" + roomId).position().top;
            if (s_top > s_bottom - 15) { s_top = s_bottom - 15; }
            else if (s_top < 0) { s_top = 0; }
            else { s_top = round(s_top  , ROUND_VALUE / 2); }
            $(".selection").css({ top: s_top + "px" });
            $(".selection").height(s_bottom - s_top);
            $("#time_counter").html(getTime(s_top));
        }
        // Update time counter
        $("#time_counter").css({ top: (e.pageY - 5) + "px", left: (e.pageX + 10) + "px" });
    })
    .mouseup(function (e) {
        flagDown = false;
        flagUp = false;
        $("#time_counter").hide();
        checkAllRooms();
    })
    .mouseleave(function (e) {
        if (flagUp || flagDown) {
            // We were dragging
            checkAllRooms();
            flagUp = false;
            flagDown = false;
            $("#time_counter").hide();
        }
    });
}