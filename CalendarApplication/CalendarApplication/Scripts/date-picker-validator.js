function validateDate(name,compare) {
    var year = document.getElementById(name + "_Year");
    var month = document.getElementById(name + "_Month");
    var day = document.getElementById(name + "_Day");
    var hour = document.getElementById(name + "_Hour");
    var minute = document.getElementById(name + "_Minute");

    // Check year
    year.value = year.value < 1 ? 1 : (year.value > 9999 ? 9999 : year.value);

    // Check month
    month.value = month.value < 1 ? 1 : (month.value > 12 ? 12 : month.value);

    var date = new Date(year.value,month.value,0);
    
    // Check day
    day.value = day.value < 1 ? 1 : (day.value > date.getDate() ? date.getDate() : day.value);
 
    // Check hours and minutes
    hour.value = hour.value < 0 ? 0 : (hour.value > 23 ? 23 : hour.value);
    minute.value = minute.value < 0 ? 0 : (minute.value > 59 ? 59 : minute.value);

    // Apply compare validation.
    var compareArr = compare.split(",");
    for (var i = 0; i < compareArr.length; i++) {
        if (compareArr[i].charAt(0) == 'g') {
            validateCompare(name, compareArr[i].substring(2), false);
        }
        else if (compareArr[i].charAt(0) == 'l') {
            validateCompare(name, compareArr[i].substring(2), true);
        }
    }

    //Writeback to the hidden field:
    var date_hidden = document.getElementById(name);
    date_hidden.value = pad(day.value) + "-" + pad(month.value) + "-" + year.value + " "
                        + pad(hour.value) + ":" + pad(minute.value) + ":00";
}

function pad(val) {
    if (val < 10) {
        return "0" + val;
    }
    else {
        return "" + val;
    }
}

function validateCompare(name,other,compGreater) {
    var year = document.getElementById(name + "_Year");
    var month = document.getElementById(name + "_Month");
    var day = document.getElementById(name + "_Day");
    var hour = document.getElementById(name + "_Hour");
    var minute = document.getElementById(name + "_Minute");
    var date = new Date();

    // Check if we should compare by today or other date
    if (other != "today") {
        date = new Date(year.value,month.value-1,day.value,hour.value,minute.value);

        year = document.getElementById(other + "_Year");
        month = document.getElementById(other + "_Month");
        day = document.getElementById(other + "_Day");
        hour = document.getElementById(other + "_Hour");
        minute = document.getElementById(other + "_Minute");

        // Reverse since we want to change the other date on error
        compGreater = !compGreater;
    }

    // Compare greater or less
    if (compGreater) {
        if (year.value >= date.getFullYear()) {
            year.value = date.getFullYear();
            if (month.value >= date.getMonth() + 1) {
                month.value = date.getMonth() + 1;
                if (day.value >= date.getDate()) {
                    day.value = date.getDate();
                    if (hour.value >= date.getHours()) {
                        hour.value = date.getHours();
                        if (minute.value < date.getMinutes()) {
                            minute.value = date.getMinutes();
                        }
                    }
                }
            }
        }
    }
    else {
        if (year.value <= date.getFullYear()) {
            year.value = date.getFullYear();
            if (month.value <= date.getMonth() + 1) {
                month.value = date.getMonth() + 1;
                if (day.value <= date.getDate()) {
                    day.value = date.getDate();
                    if (hour.value <= date.getHours()) {
                        hour.value = date.getHours();
                        if (minute.value < date.getMinutes()) {
                            minute.value = date.getMinutes();
                        }
                    }
                }
            }
        }
    }
}

/* Function for creating datepicker */
function createDatePicker(name,compare) {
    $(function () {
        $("#" + name + "_but").datepicker({
            beforeShow: function (input, inst) {
                return getRange(compare);
            },
            onSelect: function (dateText, inst) {
                var date = new Date(dateText);
                $("#" + name + "_Year").val(date.getFullYear());
                $("#" + name + "_Month").val(date.getMonth() + 1);
                $("#" + name + "_Day").val(date.getDate());
                validateDate(name, compare);
            }
        });
    });
}

/* Function for updating times in datepickers */
function getRange(cmpStr) {
    var compareArr = cmpStr.split(",");
    var maxDate = null;
    var minDate = null;
    for (var i = 0; i < compareArr.length; i++) {
        var name = compareArr[i].substring(2);
        if (compareArr[i].charAt(0) == 'g') {
            if (name == "today") {
                minDate = new Date();
            }
            else {
                minDate = $.datepicker.parseDate("dd-mm-yy", $("#" + name).val().substring(0, 10))
            }
        }
        else {
            if (name == "today") {
                maxDate = new Date();
            }
            else {
                maxDate = $.datepicker.parseDate("dd-mm-yy", $("#" + name).val().substring(0, 10))
            }
        }
    }
    return { minDate: minDate, maxDate: maxDate };
}

/* Function for showing date picker */
function showDatePicker(name) {
    $("#" + name).datepicker("show")
}