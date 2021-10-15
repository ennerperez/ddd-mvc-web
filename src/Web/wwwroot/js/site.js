// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

if (!Date.prototype.toISO8601) {
    (function () {

        function pad(number) {
            if (number < 10) {
                return '0' + number;
            }
            return number;
        }

        Date.prototype.toISO8601 = function () {
            return this.getFullYear() +
                '-' + pad(this.getMonth() + 1) +
                '-' + pad(this.getDate()) +
                'T' + pad(this.getHours()) +
                ':' + pad(this.getMinutes()) +
                ':' + pad(this.getSeconds()) +
                '.' + (this.getMilliseconds() / 1000).toFixed(3).slice(2, 5) +
                'Z';
        };

    }());
}

function delay(callback, ms) {
    var timer = 0;
    return function () {
        var context = this,
            args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}

function boolBadgeRender(input, defaultValue, trueValue, falseValue) {

    var color = "warning";
    var value = defaultValue;
    switch (input) {
        case true:
            color = "success";
            value = trueValue;
            break;
        case false:
            color = "danger";
            value = falseValue;
            break;
        default:
            break;
    }

    return `<span class="float-center badge badge-${color}">${value}</span>`;
}

function rangeBadgeRender(input, defaultValue, values, texts) {

    var color = "warning";
    var value = defaultValue;
    if (input === values[0])
    {
        color = "success";
        value = texts[0];
    }
    else if (input === values[1])
    {
        color = "danger";
        value = texts[1];
    }
    return `<span class="float-center badge badge-${color}">${value}</span>`;
}

function dateTimeRender(input, format = "DD/MM/YYYY", nullValue = '') {
    return input !== null ? moment(input).format(format) : nullValue;
}

function responsiveRender(api, rowIdx, columns) {
    var data = $.map(columns, function (col, i) {
        return col.hidden ?
            `<li data-dt-row="${col.rowIndex}" data-dt-column="${col.columnIndex}">` +
            `<span style="width:20%;"><b>${col.title}:</b> </span> ` +
            `<span>${col.data}</span>` +
            `</li>` : '';
    }).join('');

    return data ?
        $('<ul class="m-2" style="width:95%" />').append(data) :
        false;
}

let defaultButtons = [
    {
        extend: "excelHtml5",
        text: '<span class="btn-label"><i class="far fa-file-excel me-2"></i></span>Excel',
        className: "btn btn-success btn-labeled",
        action: function (e, dt, button, config) {
            Swal.fire({
                title: 'Wait...',
                text: "This process may take a few seconds depending on the number of records you want to export.",
                showCancelButton: false,
                showConfirmButton: false,
                allowOutsideClick: false,
                closeOnClickOutside: false,
            });
            var that = this;
            setTimeout(function () {
                $.fn.dataTable.ext.buttons.excelHtml5.action.call(that, e, dt, button, config);
                Swal.close();
            }, 1000);
        }
    },
    {
        extend: 'print',
        text: '<span class="btn-label"><i class="fas fa-print me-2"></i></span>Print',
        className: "btn btn-secondary btn-labeled",
        customize: function (win) {
            $(win.document).css('background-image', 'none !important');
            $(win.document.body).css('font-size', '10pt');
        }
    }
];
let defaultLengthMenu = [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "Todos"]];

function doSearchDelay(){
    $('.dataTables_filter input').unbind().bind('input', (delay(function (e) { tableMain.search($(this).val()).draw();}, 400)));
    $('[name="table_length"]').removeClass("form-control-sm").removeClass("custom-select-sm").css("min-width","6em");
}

function UIFix(){
    var container = $("#table_length").addClass("d-flex justify-content-between p-0");
    container.append('<div class="p-0" id="filter"/>');
    container.append('<div class="p-0" id="search"/>');
    container.append('<div class="p-0" id="actions"/>');

    $("#table_length label").appendTo($("#filter"));
    $("#table_filter .form-control-sm").removeClass("form-control-sm").attr("placeholder", "Search...").appendTo($("#search"));
    $("#table_filter").remove();
    $(".dt-buttons").appendTo($("#actions"));
}

function compress(file, max_width, max_height, quality) {
    if (!(/image/i).test(file.type)) {
        return false;
    }

    var compressContent = document.getElementById('compressContent');
    if (compressContent !== null) compressContent.remove();
    compressContent = document.createElement('div');
    compressContent.id = 'compressContent';
    compressContent.style = "visible: none;";
    compressContent.setAttribute("data-width", max_width);
    compressContent.setAttribute("data-height", max_height);
    compressContent.setAttribute("data-quality", quality);

    document.body.append(compressContent);

    // read the files
    var reader = new FileReader();
    reader.readAsArrayBuffer(file);

    reader.onload = function (event) {
        // blob stuff
        var blob = new Blob([event.target.result]); // create blob...
        window.URL = window.URL || window.webkitURL;
        var blobURL = window.URL.createObjectURL(blob); // and get it's URL

        // helper Image object
        var image = new Image();
        image.src = blobURL;
        //preview.appendChild(image); // preview commented out, I am using the canvas instead
        image.onload = function () {

            var container = $("#compressContent");
            var i_width = container.data("width");
            var i_height = container.data("height");
            var i_quality = container.data("quality");

            // have to wait till it's loaded
            var resized = resizeMe(image, i_width, i_height, i_quality); // send it to canvas
            var newinput = document.createElement("input");
            newinput.type = 'hidden';
            newinput.name = 'images[]';
            newinput.id = 'imageCompress';// put result from canvas into new hidden input
            newinput.value = resized;
            container.empty().append(newinput);
        }
    };

    return true;
}


function resizeMe(img, max_width, max_height, quality) {
    var canvas = document.createElement('canvas');
    var width = img.width;
    var height = img.height;
    if (max_width === null) max_width = 640;
    if (max_height === null) max_height = 480;
    if (quality === null) quality = 0.7;
    // calculate the width and height, constraining the proportions
    if (width > height) {
        if (width > max_width) {
            //height *= max_width / width;
            height = Math.round(height *= max_width / width);
            width = max_width;
        }
    } else {
        if (height > max_height) {
            //width *= max_height / height;
            width = Math.round(width *= max_height / height);
            height = max_height;
        }
    }

    // resize the canvas and draw the image data into it
    canvas.width = width;
    canvas.height = height;
    var ctx = canvas.getContext("2d");
    ctx.drawImage(img, 0, 0, width, height);
    /*$("#preview").append(canvas);*/ // do the actual resized preview
    return canvas.toDataURL("image/jpg", quality); // get the data from canvas as 70% JPG (can be also PNG, etc.)
}