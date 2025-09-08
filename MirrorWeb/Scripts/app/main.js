var intervalId;

$(document).ready(function () {

    var input = document.getElementById("navTableSearch");
    
    // Execute a function when the user presses a key on the keyboard
    input.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            event.preventDefault();
            event.stopImmediatePropagation();
            OpenTablePoPup(input.value);
        }
    });

});

//open popup for tableSearch
function OpenTablePoPup(tableName) {

    var url = "~/../../api/SnowApi/GetTableMetaData";

    //local pathname = "/"
    if (window.location.pathname === "/MirrorWeb/") {
        if (vPath != null || vPath != undefined || vPath != "") {
            url = "~/" + vPath + "/../../api/SnowApi/GetTableMetaData";
        }
    }

    //get table metadata
    $.ajax({
        url: url,
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        data: { tableName: tableName },
        success: function (data) {
            if (data !== undefined && data !== "" && data !== null) {

                var tblMetaDataDiv = document.getElementById('tableMetaData');
                tblMetaDataDiv.innerHTML = "";
                var synchronizations = data.Synchronizations;

                synchronizations.forEach(function (item) {

                    //row
                    var newDivRow = document.createElement('div');
                    newDivRow.style.cssText = "margin-bottom:10px;";
                    newDivRow.className = "row";

                    //input-group input-group-sm
                    var newDivInputGroupSm = document.createElement('div');
                    newDivInputGroupSm.className  = "input-group input-group-sm";
                    newDivRow.appendChild(newDivInputGroupSm);

                    //input-group input-group-sm
                    var newDivInputGroupPrepend = document.createElement('div');
                    newDivInputGroupPrepend.className = "input-group-prepend";
                    newDivInputGroupPrepend.style.cssText = "min-width:150px";
                    newDivInputGroupSm.appendChild(newDivInputGroupPrepend);

                    //span
                    var newSpan = document.createElement('span');
                    newSpan.className = "input-group-text";
                    newSpan.style.cssText = "width:80%";
                    newSpan.innerText = "Instance: " + item.InstanzSettings.InstanzName;
                    newDivInputGroupPrepend.appendChild(newSpan);

                    //input
                    var newInput = document.createElement('input');
                    newInput.setAttribute('type', 'text');
                    newInput.setAttribute('disabled', 'disabled');
                    newInput.setAttribute('text-align', 'center');
                    newInput.setAttribute('value', "  " + item.Name);
                    newInput.style.cssText = "width:300px;font-size:14px";
                    newDivInputGroupSm.appendChild(newInput);

                    tblMetaDataDiv.appendChild(newDivRow);
                    
                });
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });

    $("#tableSearchPopUp").kendoWindow({
        width: "550",
        minHeight: 350,
        title: "Table usage: " + tableName,
        visible: false,
        actions: ["Close"],
        close: function (e) {

        }
    }).data("kendoWindow").center().open();

    
}