/* Function for switching between pages in lists */
function gotoOtherPage(from) {
    $('#EventFrom').val(from);
    $('#list_form').submit();
}

/* Function for sorting */
function sortBy(eventOrder) {
    var old = $('#Order').val();
    if (old == eventOrder) {
        var desc = $('#Descending').val().toLowerCase() == "true";
        $('#Descending').val(!desc);
    }
    else {
        $('#Descending').val(false);
    }
    $('#Order').val(eventOrder);
    $('#EventFrom').val(0);
    $('#list_form').submit();
}

/* Function for navigating calendar (month and days).
   If date is of format yyyy-MM, the calendar month is
   shown. If it has format yyyy-MM-dd, the calendar day
   is shown. */
function goto(date) {
    var data = date.split("-");
    if (data.length < 2 || data.length > 3) { return; }
    else if (data.length == 2) {
        $("#Date").val(date + "-01 00:00:00");
        $("#Mode").val("MONTH");
    }
    else {
        $("#Date").val(date + " 00:00:00");
        $("#Mode").val("DAY");
    }
    $("#cal_form").submit();
}

/* Functions for showing/hideing event pop-ups in the calendar. */
function showPopup(name) {
    // Show full name
    $('#' + name + '_small').show();
    // Set time
    setTimeout(function () {
        // Show big if small is still visible == cursor is still on event
        if ($('#' + name + '_small').is(":visible")) {
            $('#' + name + '_big').fadeIn();
        }
    }, 700);
}

function hidePopup(name) {
    // Hide both big and small on cursor exit
    $('#' + name + '_small').hide();
}

function hidePopupBig(name) {
    $('#' + name + '_big').hide();
}

/* Function for un/checking all boxes for a given list (Eventtypes or Rooms) */
function changeCheckboxes(list,element) {
    for (var i = 0; $("#Filter_" + list + "_" + i + "__Selected").length > 0
            && $("#Filter_" + list + "_" + i + "__Selected") != null; i++) {
        $("#Filter_" + list + "_" + i + "__Selected").attr("checked", element.checked);
    }
}

/* Function to be run on filter load. Checks the checkAll box, based on the number of checked boxes (Eventtypes and Rooms) */
function checkBoxes() {
    var checked = 0;
    var unchecked = 0;
    for (var i = 0; $("#Filter_Eventtypes_" + i + "__Selected").length > 0 && $("#Filter_Eventtypes_" + i + "__Selected") != null; i++) {
        if ($("#Filter_Eventtypes_" + i + "__Selected").attr("checked")) {
            checked++;
        }
        else {
            unchecked++;
        }
    }
    $("#Eventtypes_checkAll").attr("checked", checked > unchecked);

    checked = 0;
    unchecked = 0;
    for (var i = 0; $("#Filter_Rooms_" + i + "__Selected").length > 0 && $("#Filter_Rooms_" + i + "__Selected") != null; i++) {
        if ($("#Filter_Rooms_" + i + "__Selected").attr("checked")) {
            checked++;
        }
        else {
            unchecked++;
        }
    }
    $("#Rooms_checkAll").attr("checked", checked > unchecked);
}

var roomList = [];
var roomIds = [];
var selection = false;
var dragFlag = false;
var mouseDown = false;
var s_top = 0;
var s_bottom = 0;

function addRoom(id) {
    roomList[id] = false;
    roomIds.push(id);
    roomFunctionsBeforeSelection(id);
}

function roomFunctionsBeforeSelection(roomId) {
    $("#room_" + roomId)
    .mousedown(function (e) {
        if (!selection) {
            dragFlag = false;
            mouseDown = true;
            s_top = e.pageY - $("#wrapper").position().top;
            s_bottom = s_top;
            $(this).html($(this).html() + createSelection(roomId, s_top, s_bottom - s_top));
            addDrawFunctions(roomId);
        }
        else if(!roomList[roomId]) {
            $(this).html($(this).html() + createSelection(roomId, s_top, s_bottom - s_top));
            addDrawFunctions(roomId);
        }
        roomList[roomId] = true;
    })
    .mousemove(function (e) {
        if (!selection) {
            dragFlag = mouseDown;
            if (mouseDown) {
                var y = e.pageY - $("#wrapper").position().top;
                if (y > s_top) { s_bottom = y; }
                else { s_top = y; }
                
                $(".selection").css({ top: s_top + "px" });
                $(".selection").height(s_bottom - s_top);
            }
        }
    })
    .mouseup(function (e) {
        if (!selection) {
            mouseDown = false;
            if (dragFlag) { // drag
                dragFlag = false;
                if (s_bottom - s_top < 15) {
                    s_bottom = s_top + 15;
                    $(".selection").height(s_bottom - s_top);
                }
            }
            else { // click
                s_bottom = s_top + 60;
                $(".selection").height(60);
            }
            selection = true;
        }
    });
}

function createSelection(roomId, top, height) {
    var div = "<div id='selection_" + roomId + "' class='day-event selection'";
    div += "style='top:" + top + "px;height:" + height + "px;'>"
    div += "<div class='drawer' id='" + roomId + "_b_drawer' style='bottom:2px'></div>";
    div += "<div class='drawer' id='" + roomId + "_t_drawer' style='top:2px'></div>";
    div += "</div>";
    return div;
}

var flagDown = false;
var flagUp = false;

function addDrawFunctions(roomId) {
    $("#" + roomId + "_b_drawer")
    .mousedown(function (e) {
        flagDown = true;
    })
    $("#" + roomId + "_t_drawer")
    .mousedown(function (e) {
        flagUp = true;
    })
    $("#room_" + roomId)
    .mousemove(function (e) {
        if (flagDown) {
            s_bottom = e.pageY - $("#wrapper").position().top;
            $(".selection").height(s_bottom - s_top);
        }
        else if (flagUp) {
            s_top = e.pageY - $("#wrapper").position().top;
            $(".selection").css({ top: s_top + "px" });
            $(".selection").height(s_bottom - s_top);
        }
    })
    .mouseup(function (e) {
        flagDown = false;
        flagUp = false;
    });
}

function removeAllSelections() {
    $(".selection").remove();
    for (var i = 0; i < roomIds.length; i++) { roomList[roomIds[i]] = false; }
    selection = false;
}