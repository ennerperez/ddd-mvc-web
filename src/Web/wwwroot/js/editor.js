let __editor;
let __editor_table;
let __editor_api_url;
let __editor_options = {
    delete: {
        title: "Confirmation",
        text: "Are you sure you want to delete?",
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: "Yes",
        cancelButtonText: "No"
    },
    success: {
        position: "bottom-right",
        icon: "success",
        title: "¡Done!",
    },
    error: {
        timer: 0,
        position: "bottom-right",
        icon: "error",
        title: "¡Error!"
    }
};

function setEditor(tableMain, editorMain, apiUrl, options = null) {
    __editor_table = tableMain;
    __editor = editorMain;
    __editor_api_url = apiUrl;
    if (options !== null) __editor_options = options;
}

function maskEditor() {
    let controls = $(`#${__editor._element.id} [data-mask]`);
    controls.each(function (s) {
        let control = controls[s];
        let mask = $(control).data("mask");
        let reverse = $(control).data("mask-reverse");
        let clearIfNotMatch = $(control).data("mask-clearifnotmatch");
        let placeholder = $(control).data("mask-placeholder");
        let selectOnFocus = $(control).data("mask-selectonfocus");
        let options = {
            reverse: false,
            clearIfNotMatch: false,
            placeholder: null,
            selectOnFocus: true
        };
        if (placeholder !== null && placeholder !== undefined) {
            options.placeholder = placeholder;
        }
        if (reverse !== null && reverse !== undefined) {
            options.reverse = reverse;
        }
        if (clearIfNotMatch !== null && clearIfNotMatch !== undefined) {
            options.clearIfNotMatch = clearIfNotMatch;
        }
        if (selectOnFocus !== null && selectOnFocus !== undefined) {
            options.selectOnFocus = selectOnFocus;
        }
        $(control).unmask().mask(mask.mask, options);
    });
}

function reHandle() {
    $("[name='btn-create']").each(function () {
        $(this).off("click", createRecord).on("click", createRecord);
    });
    $("[name='btn-edit']").each(function () {
        $(this).off("click", editRecord).on("click", editRecord);
    });
    $("[name='btn-delete']").each(function () {
        $(this).off("click", deleteRecord).on("click", deleteRecord);
    });
    $("[name='btn-save']").each(function () {
        $(this).off("click", saveRecord).on("click", saveRecord);
    });
}

function clearEditor() {
    let controls = $(`#${__editor._element.id} [data-field]`);
    controls.each(function (s) {
        let control = controls[s];
        let type = $(control).prop("tagName").toLowerCase();
        if ($(control).attr("type") === "checkbox") type = "checkbox";
        switch (type) {
            case "input":
            case "select":
            case "textarea":
                $(control).val("");
                break;
            case "checkbox":
                $(control).val("").prop("checked", false);
                break;
        }
        $(control).removeClass("is-invalid");
        $(control).parent().find(".invalid-feedback").remove();
    });
    let validables = $(`#${__editor._element.id} [data-validate]`);
    validables.each(function (s) {
        let control = validables[s];
        let type = $(control).prop("tagName").toLowerCase();
        switch (type) {
            case "input":
            case "textarea":
                $(control).unbind("keyup").on("keyup", function (s) {
                    if (s.keyCode === 13 || s.keyCode === 27) return;
                    $(this).removeClass("is-invalid");
                    $(this).parent().find(".invalid-feedback").remove();
                });
                break;
            case "select":
                $(control).unbind("change").on("change", function () {
                    $(this).removeClass("is-invalid");
                    $(this).parent().find(".invalid-feedback").remove();
                });
                break;
        }
    });
}

function validateEditor() {
    let is_modal_valid = true;
    let is_focused = false;
    let validables = $(`#${__editor._element.id} [data-validate]`);
    validables.each(function (s) {
        let control = validables[s];
        let data;
        let hasMask = $(control).data("mask") !== undefined;
        let dataType = $(control).data("type");
        if (dataType === "datetime" || dataType === "date" || dataType === "time")
            data = $(control).datepicker("getDate");
        else if (!hasMask)
            data = $(control).val();
        else
            data = $(control).cleanVal();

        let is_valid = (data !== null && data !== "" && data !== undefined) || $(control).attr("required") === undefined;
        let feedback = $(control).data('feedback');

        $(control).removeClass("is-invalid");
        $(control).parent().find(".invalid-feedback").remove();

        if (data !== null && data !== "" && data !== undefined) {
            let type = $(control).attr("type");
            if (type === "email") {
                const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
                is_valid = re.test(String(data).toLowerCase());
            } else if (type === "url") {
                const re = /[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)?/gi;
                is_valid = re.test(String(data).toLowerCase());
            } else {
                let pattern = $(control).data('regex');
                if (pattern !== null && pattern !== undefined) {
                    let regexp = new RegExp(pattern);
                    is_valid = regexp.test(data);
                }
            }
        }

        if (!is_valid) $(control).addClass("is-invalid");

        if (!is_valid && feedback !== null && feedback !== undefined)
            $(control).after(`<div class="invalid-feedback" style="display: block !important;">${feedback}</div>`);

        if (is_modal_valid && !is_valid) is_modal_valid = false;
        if (!is_modal_valid && !is_focused) {
            $(control).focus();
            is_focused = true;
        }

    });
    return is_modal_valid;
}

function getModel() {
    let controls = $(`#${__editor._element.id} [data-field]`);
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
                if (dataType === "datetime" || dataType === "date" || dataType === "time")
                    data = $(control).datepicker("getDate");
                else if (!hasMask)
                    data = $(control).val();
                else
                    data = $(control).cleanVal();
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

function createRecord() {
    clearEditor();
    __editor.show();
}

function editRecord() {
    clearEditor();

    let btn = $(this);
    let row = btn.closest("tr");
    let data = __editor_table.rows(row).data()[0];
    let controls = $(`#${__editor._element.id} [data-field]`);
    controls.each(function (s) {
        let control = controls[s];
        let field = $(control).data("field");
        let type = $(control).prop("tagName").toLowerCase();
        let hasMask = $(control).data("mask") !== undefined;
        let dataType = $(control).data("type");

        if ($(control).attr("type") === "checkbox") type = "checkbox";
        switch (type) {
            case "input":
            case "select":
            case "textarea":
                if (dataType === "datetime" || dataType === "date" || dataType === "time")
                    $(control).datepicker("setDate", data[field]);
                else if (!hasMask)
                    $(control).val(data[field]);
                else
                    $(control).val($(control).masked(data[field]));
                break;
            case "checkbox":
                $(control).val(data[field]).prop("checked", data[field]);
                break;
        }
    });

    __editor.show();
}

function deleteRecord() {
    let btn = $(this);
    let row = btn.closest("tr");
    let data = __editor_table.rows(row).data()[0];
    let id = parseInt(data['id']);
    swal.fire(__editor_options.delete).then((result) => {
        if (result.value) {
            let method = 'DELETE';
            let url = `${__editor_api_url}/${id}`;
            $.ajax({
                url: url,
                headers: {RequestVerificationToken: __RequestVerificationToken},
                contentType: "application/json; charset=utf-8",
                type: method,
                success: function () {
                    __editor_table.ajax.reload(reHandle);
                    Toast.fire(__editor_options.success);
                },
                error: function (e) {
                    let text = "";
                    if (e.hasOwnProperty("responseJSON")) text = e.responseJSON.detail;
                    __editor_options.error.text = text;
                    Toast.fire(__editor_options.error);
                }
            });
        }
    });
}

function saveRecord() {
    if (!validateEditor()) return;
    let model = getModel();

    let method = "POST";
    let url = `${__editor_api_url}`;
    if (model.id !== undefined) {
        method = "PUT";
        url = `${__editor_api_url}/${model.id}`;
    }

    $.ajax({
        url: url,
        headers: {RequestVerificationToken: __RequestVerificationToken},
        accepts: "application/json",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(model),
        dataType: "json",
        type: method,
        success: function () {
            __editor.hide();
            __editor_table.ajax.reload(reHandle);
            Toast.fire(__editor_options.success);
        },
        error: function (e) {
            let text = "";
            if (e.hasOwnProperty("responseJSON")) text = e.responseJSON.detail;
            __editor_options.error.text = text;
            Toast.fire(__editor_options.error);
        }
    });
}
