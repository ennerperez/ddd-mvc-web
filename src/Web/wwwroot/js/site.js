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
    let timer = 0;
    return function () {
        let context = this,
            args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}

function boolBadgeRender(input, defaultValue, trueValue, falseValue, styles) {

    let color = "secondary";
    let value = defaultValue;
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

    return `<span class="float-center badge bg-${color} ${styles}">${value}</span>`;
}

function boolPositionBadgeRender(input, position= 'top-0 start-100', styles) {

    let color = "secondary";
    switch (input) {
        case true:
            color = "success";
            break;
        case false:
            color = "danger";
            break;
        default:
            break;
    }

    return `<span class="d-print-none position-absolute translate-middle bg-${color} p-2 border border-light rounded-circle ${styles} ${position}">
                <span class="visually-hidden"></span>
            </span>`;
}

function rangeBadgeRender(input, defaultValue, values, texts, styles) {

    let color = "secondary";
    let value = defaultValue;
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
    return `<span class="float-center badge bg-${color} ${styles}">${value}</span>`;
}

function dateTimeRender(input, format = "DD/MM/YYYY", nullValue = '') {
    return input !== null ? moment(input).format(format) : nullValue;
}

function compress(file, max_width, max_height, quality) {
    if (!(/image/i).test(file.type)) {
        return false;
    }

    let compressContent = document.getElementById('compressContent');
    if (compressContent !== null) compressContent.remove();
    compressContent = document.createElement('div');
    compressContent.id = 'compressContent';
    compressContent.style = "visible: none;";
    compressContent.setAttribute("data-width", max_width);
    compressContent.setAttribute("data-height", max_height);
    compressContent.setAttribute("data-quality", quality);

    document.body.append(compressContent);

    // read the files
    let reader = new FileReader();
    reader.readAsArrayBuffer(file);

    reader.onload = function (event) {
        // blob stuff
        let blob = new Blob([event.target.result]); // create blob...
        window.URL = window.URL || window.webkitURL;
        let blobURL = window.URL.createObjectURL(blob); // and get it's URL

        // helper Image object
        let image = new Image();
        image.src = blobURL;
        //preview.appendChild(image); // preview commented out, I am using the canvas instead
        image.onload = function () {

            let container = $("#compressContent");
            let i_width = container.data("width");
            let i_height = container.data("height");
            let i_quality = container.data("quality");

            // have to wait till it's loaded
            let resized = resizeMe(image, i_width, i_height, i_quality); // send it to canvas
            let newinput = document.createElement("input");
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
    let canvas = document.createElement('canvas');
    let width = img.width;
    let height = img.height;
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
    let ctx = canvas.getContext("2d");
    ctx.drawImage(img, 0, 0, width, height);
    /*$("#preview").append(canvas);*/ // do the actual resized preview
    return canvas.toDataURL("image/jpg", quality); // get the data from canvas as 70% JPG (can be also PNG, etc.)
}

/* Model Builder */

function getModel(source) {
    let controls = $(`#${source} [data-field]`);
    let model = {};
    controls.each(function (s) {
        let control = controls[s];
        let data = null;
        let type = $(control).prop("tagName").toLowerCase();
        let hasMask = $(control).data("mask") !== undefined;
        let dataType = $(control).data("type");
        if ($(control).attr("type") === "checkbox") type = "checkbox";
        switch (type) {
            case "input":
            case "select":
            case "textarea":
                if (dataType === "datetime" || dataType === "date" || dataType === "time") {
                    data = $(control).datepicker("getDate");
                }
                else if (!hasMask) {
                    data = $(control).val();
                }
                else {
                    data = $(control).cleanVal();
                }
                break;
            case "checkbox":
                data = $(control).val("").prop("checked");
                break;
        }
        let field = $(control).data("field");
        if (data !== null && data !== "") Reflect.set(model, field, data);
    });
    return model;
}

function validateModel(source) {
    let controls = $(`#${source} [data-field]`);
    return controls.each(function (s) {
        let control = controls[s];
        let data = null;
        let type = $(control).prop("tagName").toLowerCase();
        let hasMask = $(control).data("mask") !== undefined;
        let dataType = $(control).data("type");
        let required = $(control).attr("required") !== undefined;
        if ($(control).attr("type") === "checkbox") type = "checkbox";
        switch (type) {
            case "input":
            case "select":
            case "textarea":
                if (dataType === "datetime" || dataType === "date" || dataType === "time") {
                    data = $(control).datepicker("getDate");
                } else if (!hasMask) {
                    data = $(control).val();
                } else {
                    data = $(control).cleanVal();
                }
                break;
            case "checkbox":
                data = $(control).val("").prop("checked");
                break;
        }
        if ((data === null || data === "" || data === undefined) && required) {
            $(control).focus();
            return false;
        }
        return true;
    });
}