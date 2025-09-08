var tableSyncGrid;
var tableSyncSelection = [];
var syncGridPageSize;
var TableSelection;

$(document)
    .ready(function() {

        //init settings
        tableSyncSelection = [];
        $("#btnTableSelected").show();
        $("#tableGridNext").toggle(false);
        
        syncGridPageSize = localStorage.getItem("tableSyncGridPageSize");
        if (syncGridPageSize === null || syncGridPageSize === 0 || syncGridPageSize === '0') {
            syncGridPageSize = 500;
            localStorage.setItem("tableSyncGridPageSize", syncGridPageSize);
        }

        $("#existingSynchronizationId").bind("change", function () {
            var syncGuid = $("#existingSynchronizationId option:selected").val();
            tableSyncSelection = [];
            if (syncGuid !== undefined && syncGuid !== "") {
                $("#tableGridNext").toggle(true);
                LoadExistingSynchronization(syncGuid);
            }
        });

        $("#schedulerSynchronizationId").bind("change", function () {
            var syncGuid = $("#schedulerSynchronizationId option:selected").val();
            if (syncGuid !== undefined && syncGuid !== "") {
                if (vPath != null || vPath != undefined || vPath != "") {
                    window.location.href = '/' + vPath + '/Manage/SyncScheduler?SyncId=' + syncGuid;
                } else {
                    window.location.href = '/Manage/SyncScheduler?SyncId=' + syncGuid;
                }
            }
        });
        
        
        $("#tableGridNext").bind("click", function () {
            SaveTableSelectionAndRedirect();
        });

        $("#btnLoadSync").bind("click", function () {
            if ($('#collapseExistingSyncs').is('.collapse:not(.show)')) {
                $("#btnCreateNewSync").prop("disabled", true);
            } else {
                $("#btnCreateNewSync").prop("disabled", false);
                $("#tableSyncGrid").hide();
                $("#tableGridNext").toggle(false);
            }
        });

        $("#btnCreateNewSync").bind("click", function () {
            if ($('#collapseCreateSync').is('.collapse:not(.show)')) {
                $("#btnLoadSync").prop("disabled", true);
            } else {
                $("#btnLoadSync").prop("disabled", false);
                $("#tableSyncGrid").hide();
                $("#tableGridNext").toggle(false);
            }
        });

        SetSyncTargetControl();

        $("#targetSettingId").bind("change", function () {

            SetSyncTargetControl();
        });
    });

//set target selection controls
function SetSyncTargetControl() {
    var selectedTarget = $("#targetSettingId option:selected").text();
    if (selectedTarget === "SqlDb") {
        //target is SqlDb
        $("#dbSettingRow").show();
    } else if (selectedTarget === "KafkaDev" || selectedTarget === "KafkaProd") {
        //target is Kafka
        $("#dbSettingRow").hide();
    }
}

//Load Snow table-grid
function InitializeTableSyncGrid(take, selInstanceId, selSyncId) {
    $("#tableSyncGrid").css("display", "block");

    $("#tableSyncGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 650,
        autoBind: false,
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
        filterMenuInit: initFilterMenuTableSync,
        pageable: {
            refresh: true,
            pageSizes: [500, 1000],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Tables",
                itemsPerPage: "Tables per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeTableSyncGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        selectedInstanceId: selInstanceId,
                        selSynchronizationId: selSyncId
                    }
                },
                parameterMap: function(data, type) {
                    if (type === "read") {
                        
                        return {
                            selectedInstanceId: selInstanceId,
                            selSynchronizationId: selSyncId
                        };
                    }
                }
            },
            schema: {
                data: function(result) {
                    return result.TableSyncList;
                },
                total: function(result) {
                    return result.TableSyncListTotalCount;
                },
                model: {
                    fields: {
                        Selected: { type: "boolean", editable: false },
                        name: { type: "string", editable: false },
                        UsedInOtherSync: { type: "boolean", editable: false },
                        UsedInOtherSyncList: { type: "string", editable: false }
                    }
                }
            },
            change: function(e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("tableSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("tableSyncGridGridPageSize", 10);
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
                headerTemplate: '<input type="checkbox" id="headerInstall-chb" class="k-checkbox" onclick="SelectUnselectSyncFlags();" style="padding: 0;"><label class="k-checkbox-label" for="headerInstall-chb" style="font-weight: normal; text-align:center; padding: 0"></label>',
                attributes: { style: "text-align:right" },
                template: '<input type="checkbox" #= Selected ? \'checked="checked"\' : "" # id="snowTable-chb_#= sys_id#" class="k-checkbox" onclick="SelectUnselectSingleFlag($(this));" /><label class="k-checkbox-label" for="snowTable-chb_#= sys_id#"></label>',
                width: 6,
                filterable: false,
                sortable: false
            }, {
                field: "name",
                title: "TableName",
                template: "#if (UsedInOtherSync == false) { # <span>#=name#</span> # } else" +
                          " { # <span style='color: red;'>#=name#</span> # } #",
                width: 70
            }, {
                field: "UsedInOtherSyncList",
                title: "Used In Other Sync",
                width: 80
            }
        ]
    }).data("kendoGrid");

    tableSyncGrid = $("#tableSyncGrid").data("kendoGrid");
    tableSyncGrid.bind("dataBound", tableSyncGridDataBound);
    tableSyncGrid.dataSource.fetch();
}

//init filter
function initFilterMenuTableSync(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function(ev) {

        $("#headerInstall-chb").prop("checked", false);

        var fieldType = getFieldType(e.sender.dataSource, e.field);

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

    $('button[type="reset"]').click(function(ev) {
        $("#headerInstall-chb").prop("checked", false);
    });
}

function getFieldType(dataSource, field) {
    return dataSource.options.schema.model.fields[field].type;
}

//Set active/inactive filter forecolor 
function tableSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembers(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function() {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}
//Set filtered members
function setFilteredMembers(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

//Select/Unselect clients from current page
function SelectUnselectSyncFlags() {
    var headerChecked = $("#headerInstall-chb").prop("checked");
    var dataSource = $("#tableSyncGrid").data("kendoGrid").dataSource;
    var filters = dataSource.filter();
    var allData = dataSource.data();
    var query = new kendo.data.Query(allData);
    var data = query.filter(filters).data;

    kendo.ui.progress($("#tableSyncGrid"), true);

    $.each(data, function(i, row) {
        var selTable = new Object();
        selTable.SysId = row.sys_id;
        selTable.Name = row.name;
        selTable.UsedInOtherSync = row.UsedInOtherSync;

        if (headerChecked) {
            $("#snowTable-chb_" + row.sys_id).prop("checked", true);
            if (tableSyncSelection.findIndex(x => x.SysId === row.sys_id) < 0) {
                tableSyncSelection.push(selTable);
            }
        } else {
            $("#snowTable-chb_" + row.sys_id).prop("checked", false);
            if (tableSyncSelection.findIndex(x => x.SysId === row.sys_id) >= 0) {
                tableSyncSelection = jQuery.grep(tableSyncSelection, function (value) {
                    return value.SysId !== row.sys_id;
                });
            }
        }
    });

    if (tableSyncSelection.length > 0) {
        $("#tableGridNext").toggle(true);
    } else {
        $("#tableGridNext").toggle(false);
    }

    kendo.ui.progress($("#tableSyncGrid"), false);
}

//grid checkbox selection
function SelectUnselectSingleFlag(object) {

    var selItem = object[0];
    kendo.ui.progress($("#tableSyncGrid"), true);
    
    var changedSelection = selItem.checked, row = selItem.closest("tr"),
        grid = $("#tableSyncGrid").data("kendoGrid"),
        tableItem = grid.dataItem(row);

    if (tableItem !== null && tableItem !== undefined) {
        var selTable = new Object();

        if (selItem.checked) {
            //add if id is not in list  
            if (tableSyncSelection.findIndex(x => x.SysId === tableItem.sys_id) < 0) {
                selTable.SysId = tableItem.sys_id;
                selTable.Name = tableItem.name;
                selTable.UsedInOtherSync = tableItem.UsedInOtherSync;
                tableSyncSelection.push(selTable);
            }
        } else {
            //remove id if it is in list
            if (tableSyncSelection.findIndex(x => x.Name === tableItem.name) >= 0) {
                tableSyncSelection = jQuery.grep(tableSyncSelection, function (value) {
                    return value.Name !== tableItem.name;
                });
            }
        }
    }

    if (tableSyncSelection.length > 0) {
        $("#tableGridNext").toggle(true);
    } else {
        $("#tableGridNext").toggle(false);
    }
    
    kendo.ui.progress($("#tableSyncGrid"), false);
}

//Load existing Synchronization
function LoadExistingSynchronization(syncId) {
    if (syncId !== "") {

        $.ajax({
            url: "~/../../api/SnowApi/LoadExistingSynchronization",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { synchronizationId: syncId },
            success: function(res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //prepare style and fields
                    $("#tableSyncGrid").css("visibility", "visible");
                    $("#selSyncId").val(syncId);

                    //set tableSelection
                    res.SnowTables.forEach(function (tableItem) {
                        if (tableSyncSelection.findIndex(x => x.Name === tableItem.Name) < 0) {
                            var selTable = new Object();
                            selTable.Name = tableItem.Name;
                            selTable.UsedInOtherSync = tableItem.UsedInOtherSync;
                            tableSyncSelection.push(selTable);
                        }
                    });
                    
                    InitializeTableSyncGrid(syncGridPageSize, res.SelectedInstanceId, res.SynchronizationId);
                }
            },
            error: function(data) {

            }
        });

    }
}

//create new synchronization entry
function CreateNewSynchronization() {

    var selSyncName = $("#selSyncName").val();
    var selIstanceId = $("#selIstanceSettings").val();
    var selDbSettingId = $("#selDatabaseSettings").val();
    var selTarget = $("#targetSettingId option:selected").text();
    var selTargetId = $("#targetSettingId").val();

    if (selSyncName === null || selSyncName === undefined || selSyncName === "") {
        alert("Missing Synchronization Name");
        return;
    }

    if (selIstanceId === null || selIstanceId === undefined || selIstanceId === "") {
        alert("Missing SNOW Instance");
        return;
    }

    //1=Sql, 2=Kafka
    if (selTarget === "SqlDb") {
        if (selDbSettingId === null || selDbSettingId === undefined || selDbSettingId === "") {
            alert("Missing Database");
            return;
        }
    }
    
    $.ajax({
        url: "~/../../api/SnowApi/CreateNewSynchronization",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        data: { syncName: selSyncName, syncInstanceId: selIstanceId, syncDatabaseId: selDbSettingId, syncTargetId: selTargetId  },
        success: function (res) {
            if (res !== undefined && res !== "" && res !== null) {
                $("#selSyncId").val(res.SynchronizationId);
                InitializeTableSyncGrid(syncGridPageSize, selIstanceId, res.SynchronizationId);
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}

//add or update synchronization tableselection and redirect
function SaveTableSelectionAndRedirect() {

    var syncId = $("#selSyncId").val();
    var syncObject = new Object();
    syncObject.SynchronizationId = syncId;
    syncObject.SnowTables = tableSyncSelection;

    $.ajax({
        type: "POST",
        url: "~/../../api/SnowApi/SaveTableSelectionAndRedirect",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: JSON.stringify(syncObject),
        success: function (res) {
            if (res.Success) {
                window.location.href = res.RedirectUrl;
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}