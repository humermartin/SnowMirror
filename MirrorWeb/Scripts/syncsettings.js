var columnSyncGrid;
var columnSyncSelection = [];
var syncGridPageSize;
var scriptCommandsGrid;
var scriptCommandsGridPageSize;

$(document)
    .ready(function () {

        syncGridPageSize = localStorage.getItem("columnSyncGridPageSize");
        if (syncGridPageSize === null || syncGridPageSize === 0 || syncGridPageSize === '0') {
            syncGridPageSize = 10;
            localStorage.setItem("columnSyncGridPageSize", syncGridPageSize);
        }

        scriptCommandsGridPageSize = localStorage.getItem("scriptCommandsGridPageSize");
        if (scriptCommandsGridPageSize === null || scriptCommandsGridPageSize === 0 || scriptCommandsGridPageSize === '0') {
            scriptCommandsGridPageSize = 10;
            localStorage.setItem("scriptCommandsGridPageSize", scriptCommandsGridPageSize);
        }

        $("#chkAutoSchemaUpdate")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#autoSchemaUpdateSelected").val(state);
                }
            });

        $("#syncSettingsNext").bind("click", function () {
            UpdateSyncSettings();
        });

        $("#selectTableColumns").bind("change", function () {
            GetTableColumns($(this).val(), $("#synchronizationGuid").val());
        });

        $("#snowScriptsGrid").on("click", "#deleteCommand", function (e) {
            e.preventDefault();
            var row = $(this).closest("tr");
            var grid = $("#snowScriptsGrid").data("kendoGrid");
            var commandItem = grid.dataItem(row);
            DeleteScriptCommand(commandItem.Id);
        });
});

function SetSchedulerButton() {
    var kSelectedListBox = $("#selected").data('kendoListBox');

    if (kSelectedListBox.dataSource.data().length > 0) {
        $("#btnColumnsSelected").show();
    } else {
        $("#btnColumnsSelected").hide();
    }
}

//update sync settings
function UpdateSyncSettings() {
    
    var syncSettings = new Object();
    syncSettings.SynchronizationId = $("#synchronizationGuid").val();
    syncSettings.AutoSchemaUpdate = $("#chkAutoSchemaUpdate")[0].checked;
    
    if (syncSettings.SynchronizationId !== null && syncSettings.SynchronizationId !== undefined && syncSettings.SynchronizationId !== "") {

        $.ajax({
            type: "POST",
            url: "~/../../api/SnowApi/UpdateSyncSettings",
            dataType: "json",
            contentType: "application/json; charset=utf8",
            data: JSON.stringify(syncSettings),
            success: function(res) {
                if (res.Success) {

                    window.location.href = res.RedirectUrl;
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alertify.error(err);
            }
        });
    } else {
        alertify.error("Cannot update settings because SynchronizationId is null");
    }
    
}

function GetTableColumns(selectedTable, syncId) {

    if (selectedTable !== null && selectedTable !== undefined && selectedTable !== "") {
        //load columns grid
        InitializeColumnSyncGrid(syncGridPageSize, selectedTable, syncId);

        //load scriptcommands grid
        InitializeScriptCommandsGrid(syncGridPageSize, selectedTable, syncId);

        $("#saveColChangesId").show();
    } else {
        $("#snowColumnsGrid").data("kendoGrid").dataSource.data([]);
        $("#saveColChangesId").hide();
    }
}

//Load Snow table-grid
function InitializeColumnSyncGrid(take, selectedTable, syncId) {
    $("#snowColumnsGrid").css("display", "block");

    $("#snowColumnsGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 650,
        autoBind: false,
        toolbar: [
            {
                template: '<a id="saveColChangesId" class="btn btn-sm custHeaderBackground" href="\\#" onclick="return SaveColumnSelection()" style="font-size: 12px; color: white;">Save changes</a>'
            }
        ],
        filterable: {
            operators: {
                string: {
                    eq: "Equal to",
                    neq: "Not equal to",
                    isnull: "Null",
                    isnotnull: "Not null",
                    contains: "Contains",
                    doesnotcontain: "Doesn't contain",
                    startswith: "Starts",
                    endswith: "Ends",
                    doesnotstartwith: "Does not start",
                    doesnotendwith: "Does not end",
                    isempty: "Empty",
                    isnotempty: "Not empty"
                }
            }
        },
        filterMenuInit: initFilterMenuColumnSync,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50, 100],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Columns",
                itemsPerPage: "Columns per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeColumnGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        tableName: selectedTable,
                        synchronizationId: syncId
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                            tableName: selectedTable,
                            synchronizationId: syncId
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    LoadColumnSelection(result.SnowColumns);
                    return result.SnowColumnList;
                },
                total: function (result) {
                    return result.SnowColumnListTotalCount;
                },
                model: {
                    fields: {
                        Selected: { type: "boolean", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("columnSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("columnSyncGridGridPageSize", 10);
                }
                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true,
            pageSize: take
        },
        editable: true,
        columns: [
            {
                field: "Selected",
                headerTemplate: '<input type="checkbox" id="headerInstall-chb" class="k-checkbox" onclick="SelectUnselectColumns();" style="padding: 0;"><label class="k-checkbox-label" for="headerInstall-chb" style="font-weight: normal; text-align:center; padding: 0"></label>',
                attributes: { style: "text-align:left" },
                template: '<input type="checkbox" #= Selected ? \'checked="checked"\' : "" # id="snowColumn-chb_#= sys_id.value#" class="k-checkbox" onclick="SelectUnselectSingleColumn($(this));" /><label class="k-checkbox-label" for="snowColumn-chb_#= sys_id.value#"></label>',
                width: 6,
                filterable: false,
                sortable: false
            }, {
                field: "element.value",
                title: "Name",
                width: 60
            }, {
                field: "column_label.value",
                title: "Label",
                width: 60
            }, {
                field: "max_length.value",
                title: "MaxLength",
                width: 30,
                filterable: false
            }, {
                field: "internal_type.value",
                title: "Type",
                width: 60
            }
        ]
    }).data("kendoGrid");

    columnSyncGrid = $("#snowColumnsGrid").data("kendoGrid");
    columnSyncGrid.bind("dataBound", columnSyncGridDataBound);
    columnSyncGrid.dataSource.fetch();
}

//init filter
function initFilterMenuColumnSync(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        $("#headerInstall-chb").prop("checked", false);

        var fieldType = getGridColumnFieldType(e.sender.dataSource, e.field);

        if (firstDropDown.value() === 'eq' && fieldType === "date") {
            ev.preventDefault();
            var selectedDate = $('[data-role="datepicker"]').first().data('kendoDatePicker').value();

            if (!selectedDate) {
                $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
                return;
            }

            var startOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate());
            var startOfFilterDate = moment(startOfFilterDateUf).format('DD-MM-YYYY');

            var endOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), 23, 59, 59);
            var endOfFilterDate = moment(endOfFilterDateUf).format('DD-MM-YYYY');

            var filter = {
                filters: [
                    { field: e.field, operator: "gte", value: startOfFilterDate },
                    { field: e.field, operator: "lte", value: endOfFilterDate }
                ]
            };
            e.sender.dataSource.filter(filter);
            $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
            return;
        }
    });

    $('button[type="reset"]').click(function (ev) {
        $("#headerInstall-chb").prop("checked", false);
    });
}

function getGridColumnFieldType(dataSource, field) {
    return dataSource.options.schema.model.fields[field].type;
}

//Set active/inactive filter forecolor 
function columnSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersSyncSettings(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}
//Set filtered members
function setFilteredMembersSyncSettings(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

//select-unselect columns from current page
function SelectUnselectColumns() {
    var headerChecked = $("#headerInstall-chb").prop("checked");
    var dataSource = $("#snowColumnsGrid").data("kendoGrid").dataSource;
    var filters = dataSource.filter();
    var allData = dataSource.data();
    var query = new kendo.data.Query(allData);
    var data = query.filter(filters).data;
    
    $.each(data, function (i, row) {
        var selColumn = new Object();
        selColumn.SysId = row.sys_id.value;
        selColumn.Name = row.element.value;
        
        if (headerChecked) {
            $("#snowColumn-chb_" + row.sys_id.value).prop("checked", true);
            if (columnSyncSelection.findIndex(x => x.SysId === row.sys_id.value) < 0) {
                columnSyncSelection.push(selColumn);
            }
        } else {
            $("#snowColumn-chb_" + row.sys_id.value).prop("checked", false);
            if (columnSyncSelection.findIndex(x => x.SysId === row.sys_id.value) >= 0) {
                columnSyncSelection = jQuery.grep(columnSyncSelection, function (value) {
                    return value.SysId !== row.sys_id.value;
                });
            }
        }
    });
}

//grid checkbox single column selection
function SelectUnselectSingleColumn(object) {

    var selItem = object[0];
    
    var changedSelection = selItem.checked, row = selItem.closest("tr"),
        grid = $("#snowColumnsGrid").data("kendoGrid"),
        columnItem = grid.dataItem(row);

    if (columnItem !== null && columnItem !== undefined) {
        var selColumn = new Object();

        if (selItem.checked) {
            //add if id is not in list  
            if (columnSyncSelection.findIndex(x => x.SysId === columnItem.sys_id.value) < 0) {
                selColumn.SysId = columnItem.sys_id.value;
                selColumn.Name = columnItem.element.value;
                columnSyncSelection.push(selColumn);
            }
        } else {
            //remove id if it is in list
            if (columnSyncSelection.findIndex(x => x.Name === columnItem.element.value) >= 0) {
                columnSyncSelection = jQuery.grep(columnSyncSelection, function (value) {
                    return value.Name !== columnItem.element.value;
                });
            }
        }
    }
}

function LoadColumnSelection(snowColumns) {
    //set columnSyncSelection
    if (snowColumns !== null && snowColumns !== undefined) {
        snowColumns.forEach(function (colItem) {
            if (columnSyncSelection.findIndex(x => x.Name === colItem) < 0) {
                var selColumn = new Object();
                selColumn.Name = colItem;
                columnSyncSelection.push(selColumn);
            }
        });
    }
}

function SaveColumnSelection() {

    var syncId = $("#synchronizationGuid").val();
    var tableName = $("#selectTableColumns").val();
    
    var jObject = new Object();
    jObject.SynchronizationId = syncId;
    jObject.TableName = tableName;
    jObject.SnowColumns = columnSyncSelection.map(a => a.Name);;

    $.ajax({
        type: "POST",
        url: "~/../../api/SnowApi/SaveColumnSelection",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: JSON.stringify(jObject),
        success: function (res) {
            if (res.Success) {
                alertify.success("Column changes saved for table " + tableName);
            } else {
                 alertify.error("Could not save/upate columns tabledefiniton for table " + tableName);
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });

    columnSyncSelection = [];
    return false;
}

//Load Snow table-grid
function InitializeScriptCommandsGrid(take, selectedTable, syncId) {
    $("#snowScriptsGrid").css("display", "block");

    $("#snowScriptsGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
        autoBind: false,
        toolbar: [
            {
                template: '<div style="margin-right: 10px;"><div class="input-group"><div class="input-group-prepend"><span class="input-group-text" style="font-size: 12px;">Enter command</span></div><textarea id="scriptCommandAreaId" class="form-control" style="font-size: 12px; max-width: 100%; width: 80%"></textarea><a id="scriptCommandId" class="btn btn-sm custHeaderBackground" href="\\#" onclick="return AddScriptCommand()" style="font-size: 12px; color: white; width:50px; line-height: 50px;">Add</a></div></div>'
            }
        ],
        filterable: {
            operators: {
                string: {
                    eq: "Equal to",
                    neq: "Not equal to",
                    isnull: "Null",
                    isnotnull: "Not null",
                    contains: "Contains",
                    doesnotcontain: "Doesn't contain",
                    startswith: "Starts",
                    endswith: "Ends",
                    doesnotstartwith: "Does not start",
                    doesnotendwith: "Does not end",
                    isempty: "Empty",
                    isnotempty: "Not empty"
                }
            }
        },
        filterMenuInit: initFilterMenuScriptCommands,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Script(s)",
                itemsPerPage: "Scripts per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeScriptCommandsGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        tableName: selectedTable,
                        synchronizationId: syncId
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                            tableName: selectedTable,
                            synchronizationId: syncId
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    return result.ScriptCommandList;
                },
                total: function (result) {
                    return result.ScriptCommandListTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false },
                        Table: { type: "string", editable: false },
                        Command: { type: "string", editable: false },
                        Created: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("scriptCommandsGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("scriptCommandsGridPageSize", 10);
                }
                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true,
            pageSize: take
        },
        editable: true,
        columns: [
            {
                field: "Id",
                title: "Id",
                hidden: true
            }, {
                field: "TableName",
                title: "Table",
                width: 40
            }, {
                field: "Command",
                title: "Command",
                width: 150
            }, {
                field: "Created",
                title: "Created",
                width: 30,
                filterable: false
            }, {
                width: 30,
                template: "<button class='k-button' id='deleteCommand'>Delete</button>",
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    scriptCommandsGrid = $("#snowScriptsGrid").data("kendoGrid");
    scriptCommandsGrid.bind("dataBound", scriptCommandsGridDataBound);
    scriptCommandsGrid.dataSource.fetch();
}

//init filter
function initFilterMenuScriptCommands(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        var fieldType = getGridColumnFieldType(e.sender.dataSource, e.field);

        if (firstDropDown.value() === 'eq' && fieldType === "date") {
            ev.preventDefault();
            var selectedDate = $('[data-role="datepicker"]').first().data('kendoDatePicker').value();

            if (!selectedDate) {
                $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
                return;
            }

            var startOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate());
            var startOfFilterDate = moment(startOfFilterDateUf).format('DD-MM-YYYY');

            var endOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), 23, 59, 59);
            var endOfFilterDate = moment(endOfFilterDateUf).format('DD-MM-YYYY');

            var filter = {
                filters: [
                    { field: e.field, operator: "gte", value: startOfFilterDate },
                    { field: e.field, operator: "lte", value: endOfFilterDate }
                ]
            };
            e.sender.dataSource.filter(filter);
            $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
            return;
        }
    });
}

//Set active/inactive filter forecolor 
function scriptCommandsGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersSyncSettings(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}

function AddScriptCommand() {
    
    var scriptCommandObject = new Object();
    scriptCommandObject.SynchronizationId = $("#synchronizationGuid").val();
    scriptCommandObject.TableName = $("#selectTableColumns").val();
    scriptCommandObject.Command = $("#scriptCommandAreaId").val();

    $.ajax({
        url: "~/../../api/SnowApi/AddScriptCommand",
        dataType: "json",
        contentType: "application/json",
        type: 'POST',
        data: JSON.stringify(scriptCommandObject),
        success: function(res) {
            InitializeScriptCommandsGrid(syncGridPageSize, scriptCommandObject.TableName, scriptCommandObject.SynchronizationId);
        },
        error: function(xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}

function DeleteScriptCommand(commandId) {
    var syncId = $("#synchronizationGuid").val();
    var tableName = $("#selectTableColumns").val();
    
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/DeleteScriptCommand",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { synchronizationId: syncId, tableName: tableName, scriptCommandId: commandId },
        success: function (res) {
            InitializeScriptCommandsGrid(syncGridPageSize, tableName, syncId);
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}