var syncQueueGrid;
var syncQueue = [];
var intervalId;
var gridPageSize;
var gbSelSyncId;
var filterQuery;

$(document)
    .ready(function () {

        $('#rerunEnabled').on('switchChange.bootstrapSwitch', function (e, data) {
        });

        $('#rerunTableInheritance').on('switchChange.bootstrapSwitch', function (e, data) {
        });

        $("#editCustomDeltaStart").bind("click", function () {
            EditCustomDeltaStart();
        });

        kendo.culture("en-EN");
        
        $("#rerunCustomDeltaStart, #paramSetCustomDeltaStart").kendoDateTimePicker({
            timeFormat: "HH:mm:ss",
            format: "dd.MM.yyyy HH:mm:ss",
            parseFormats: ["dd.MM.yyyy hh:mm:sstt", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "HH:mm:ss"]
        });
        
        //init settings
        syncQueue = [];

        //init dropdown
        SelectSyncName();
    });

//Start test sync
function StartTestSync() {

    var selTargetType = "Sql";
    var syncId = $("#syncQueueListTargetSqlDb option:selected").val();

    if (syncId == undefined || syncId === "") {
        syncId = $("#syncQueueListTargetKafka option:selected").val();
        selTargetType = "Kafka";
    }
    
    $.ajax({
        url: "~/../../api/SnowApi/StartSelectedSyncProcess",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        data: { synchronizationId: syncId },
        success: function (res) {
            var gridConfig = GetColVisibility();
            InitializesyncQueueGrid(0, gridPageSize, syncId, selTargetType, gridConfig);
        },
        error: function (data) {

        }
    });
}

//Selected SyncName
function SelectSyncName(selTarget) {

    var selSyncId = "";
    
    if (selTarget === "Sql") {
        //reset kafka dropdown
        $("#syncQueueListTargetKafka").val("");
        selSyncId = $("#syncQueueListTargetSqlDb").val();
    } else if (selTarget === "Kafka") {
        //reset sql dropdown
        $("#syncQueueListTargetSqlDb").val("");
        selSyncId = $("#syncQueueListTargetKafka").val();
    }
    
    if (selSyncId !== undefined && selSyncId !== null && selSyncId !== "") {
        $("#curSyncParameters").css("display", "block");

        gridPageSize = localStorage.getItem("syncQueueGridPageSize");
        if (gridPageSize === null || gridPageSize === 0 || gridPageSize === '0') {
            gridPageSize = 20;
            localStorage.setItem("syncQueueGridPageSize", gridPageSize);
        }

        if ($('#syncQueueGrid').length > 0) {
            $("#syncQueueGrid").empty();
            var gridConfig = GetColVisibility();
            InitializesyncQueueGrid(0, gridPageSize, selSyncId, selTarget, gridConfig);
            gbSelSyncId = selSyncId;
        }
    } else {
        $("#syncQueueGrid").hide();
    }
}

//set current sync header information
function SetSyncHeaderInfo(model) {

    $("#curSyncType").val(model.SyncType.TypeName);
    $("#curSyncInterval").val(model.SelectedIntervalName);
    $("#curIntervalMinutes").val(model.IntervalInMinutes);
    $("#curSyncAutoSchemaUpdate").val(model.AutoSchemaUpdate);
    $("#curMaxThreads").val(model.MaxThreads);
    $("#curThreadsPerTable").val(model.ThreadsPerTable);
    $("#curThreadSleepTime").val(model.ThreadSleepTime);
    $("#curRequestTimeout").val(model.RequestTimeout);
    $("#curMaxErrorsPerPage").val(model.MaxErrorsPerPage);
    $("#curPageSize").val(model.PageSize);
    $("#kafkaBlockSize").val(model.KafkaBlockSize);
    $("#kafkaMode").val(model.KafkaMode);
    if (model.SyncTarget.TargetType === "Sql") {
        $("#curSyncDatabaseServer").val(model.SelectedDatabaseSettings.Servername);
        $("#curSyncDatabase").val(model.SelectedDatabaseSettings.Instancename);
    }
    $("#curSyncTarget").val(model.SyncTarget.TargetType);
    $("#curSyncEndpoint").val(model.SyncTarget.Endpoint);

    $("#curSyncSnowInstance").val(model.SelectedInstanzSettings.InstanzName);
    if (model.SyncType.TypeName === 'Delta') {
        $("#custDeltaStartRow").show();
        $("#custDeltaStartTime").val(model.CustomDeltaStart);
    } else {
        $("#custDeltaStartRow").hide();
    }
    $("#curSyncStartTime").val(model.StartTime);
    $("#curSyncEndTime").val(model.EndTime);
    $("#curSyncFinalMessage").val(model.FinalMessage);
    $("#curSyncFinalErrorMessage").val(model.FinalErrorMessage);
    $("#curSyncCreated").val(model.Created);
    $("#curNextPlannedSync").val(model.NextPlannedSync);
    
    if (model.IsAdmin === false) {
        $("#syncQueueGrid").find(".k-grid-toolbar").hide();
    } else {
        $("#syncQueueGrid").find(".k-grid-toolbar").show();
    }
}

function InitializesyncQueueGrid(skip, take, selectedSyncId, targetType, gridConfig) {
    $("#syncQueueGrid").css("display", "block");

    var elSync;
    if (targetType === "Kafka") {
        elSync = document.getElementById("syncQueueListTargetKafka");
    } else {
        elSync = document.getElementById("syncQueueListTargetSqlDb");
    }

    //set column default visibility
    var hideColInserted = false;
    var hideColSnowCount = false;
    var hideColRecordCount = false;
    var hideColSqlCount = false;
    var colUpdatedName = "Updated";

    if (gridConfig != undefined) {
        hideColSqlCount = !gridConfig.EnableColumnSqlCount;
        hideColRecordCount = !gridConfig.EnableColumnRecordCount;
        hideColSnowCount = !gridConfig.EnableColumnSnowCount;
    }
    
    if (targetType === "Kafka") {
        hideColInserted = true;
        hideColSnowCount = true;
        hideColRecordCount = false;
        hideColSqlCount = true;
        colUpdatedName = "Posted";
    }
    
    $("#syncQueueGrid").kendoGrid({
        toolbar: kendo.template($("#retrySelectedTablesTemplate").html()),
        excel: {
            fileName: elSync.options[elSync.selectedIndex].text + ".xlsx",
            allPages: true
        },
        pdf: {
            fileName: elSync.options[elSync.selectedIndex].text + ".pdf",
            allPages: true
        },
        groupable: true,
        resizable: false,
        scrollable: true,
        height: 650,
        autoBind: false,
        filterable: {
            extra: false,
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
        sortable: {
            mode: "single",
            allowUnsort: false
        },
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50, 100, 500, 1000],
            previousNext: true,
            width: 1006,
            buttonCount: 10,
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
                    url: "~/../../api/SnowApi/InitializeSyncQueueGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        synchronizationId: selectedSyncId,
                        skip: skip,
                        take: take,
                        filter: null
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        var skip = (data.page * data.pageSize) - data.pageSize;

                        filterQuery = null;
                        
                        if (data.filter !== 0 && data.filter !== null) {
                            filterQuery = JSON.stringify(data.filter);
                        }

                        return {
                            synchronizationId: selectedSyncId,
                            skip: skip,
                            take: data.pageSize,
                            filter: filterQuery
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    SetSyncHeaderInfo(result);
                    return result.SnowTables;
                },
                total: function (result) {
                    return result.SnowTablesCount;
                },
                model: {
                    fields: {
                        Select: { type: "string", editable: false },
                        Suspend: { type: "string", editable: false },
                        Process: { type: "string", editable: false },
                        ReRun: { type: "string", editable: false },
                        Name: { type: "string", editable: false },
                        SnowCount: { type: "number", editable: false },
                        RowCount: { type: "number", editable: false },
                        SqlCount: { type: "number", editable: false },
                        Progress: { type: "string", editable: false },
                        Inserted: { type: "number", editable: false },
                        Updated: { type: "number", editable: false },
                        Deleted: { type: "number", editable: false },
                        Failures: { type: "number", editable: false },
                        SyncTime: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("syncQueueGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("syncQueueGridPageSize", 10);
                }
                e.preventDefault();
            },

            serverPaging: true,
            serverFiltering: true,
            serverSorting: false,
            pageSize: take
        },
        editable: false,
        columns: [
            {
                headerTemplate:
                    '<input type="checkbox" id="headerQueue-chb" class="k-checkbox" onclick="SelectUnselectAllFlag();" style="padding: 0;"><label class="k-checkbox-label" for="headerQueue-chb" style="font-weight: normal; text-align:center; padding: 0"></label>',
                attributes: { style: "text-align:left;" },
                template: "#if (UsedInOtherSync == true) {#  #} else if" +
                    "(SyncState != 2 && UsedInOtherSync == false) { # <input type='checkbox' id='queueTable-chb_#=kendo.toString(replaceString(Name))#' class='k-checkbox' onclick='SelectUnselectTable($(this));' style='padding: 0' /><label class='k-checkbox-label' for='queueTable-chb_#=kendo.toString(replaceString(Name)) #' style='font-weight: normal; text-align: center; padding: 0'></label> # } else if" +
                    "(UsedInOtherSync == false) { # <input type='checkbox' disabled id='queueTable-chb_#=kendo.toString(replaceString(Name))#' class='k-checkbox' style = 'padding: 0' /><label class='k-checkbox-label' for='queueTable-chb_#=kendo.toString(replaceString(Name)) #'></label> # } #",
                width: 6,
                sortable: false,
                filterable: false
            }, {
                title: "S/C",
                attributes: { style: "text-align:center" },
                template: "#if (UsedInOtherSync == true) {#  #} else if" +
                    " (SyncState == 2 && UsedInOtherSync == false) { # <span id='suspend_#=kendo.toString(replaceString(Name))#' class='fas fa-pause-circle' title='Suspend - continue is possible' style='font-size: 15px; color: red; cursor: pointer;'></span> # } else if" +
                    " (SyncState == 7 && UsedInOtherSync == false) { # <span id='suspend_#=kendo.toString(replaceString(Name))#' class='fas fa-play-circle' title='Continue' style='font-size: 16px; color: green; cursor: pointer;'></span> # } else" +
                    " { # <span id='suspend_#=kendo.toString(replaceString(Name))#'></span> # } #",
                width: 9,
                filterable: false
            }, {
                attributes: { style: "text-align:center" },
                template: "#if (UsedInOtherSync == true) {#  #} else if" +
                    " (SyncState == 2 && UsedInOtherSync == false) { # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fa fa-stop-circle' onclick='StopSyncProcess($(this))'; title='Interrupt process - continue not possible' style='font-size: 16px; color: red; cursor: pointer;'></span> # } else if" +
                    " (SyncState == 3 && UsedInOtherSync == false) { # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fas fa-check-circle' style='font-size: 16px; color: green;'></span> # } else if" +
                    " (SyncState == 4 && UsedInOtherSync == false) { # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fas fa-trash-alt' title='Cleanup process' style='font-size: 16px; color: darkorange; cursor: pointer;'></span> # } else if" +
                    " (SyncState == 5 && UsedInOtherSync == false) { # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fas fa-exclamation-circle' style='font-size: 16px; color: red;'></span> # } else if" +
                    " (SyncState == 6 && UsedInOtherSync == false) { # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fas fa-frown' title='Cleanup process' style='font-size: 16px; color: red; cursor: pointer;'></span> # } else if" +
                    " (UsedInOtherSync == false){ # <span id='syncState_#=kendo.toString(replaceString(Name))#' class='fas fa-play-circle' style='font-size: 16px; color: darkblue;'></span> # } #",
                width: 8,
                filterable: false
            }, {
                template: "#if (UsedInOtherSync == true) {# <span id='rerun_#=kendo.toString(replaceString(Name))#'></span> #} else if" +
                    " (UsedInOtherSync == false){ # <span id='rerun_#=kendo.toString(replaceString(Name))#' class='fas fa-cog' onclick='SetReRunParams($(this));' style='font-size: 16px; color: brown; cursor: pointer'></span> # } else" +
                    " { # <span id='rerun_#=kendo.toString(replaceString(Name))#'></span> # } #",
                width: 8,
                filterable: false
            }, {
                field: "Name",
                title: "TableName",
                template: "#if (UsedInOtherSync == false && Enabled == true) { # <span id='tableName_#=kendo.toString(replaceString(Name))#' style='float: left;'>#=Name#</span><div class='fas fa-info-circle' id='tooltip_#=kendo.toString(replaceString(Name))#' title='#=ProcessMessage#' style='float: right; font-size: 16px; color: grey; cursor: pointer;'></div> # } else if" +
                          " (UsedInOtherSync == false && Enabled == false) { # <span id='tableName_#=kendo.toString(replaceString(Name))#' style='float: left; color: silver;'>#=Name#</span><div class='fas fa-info-circle' id='tooltip_#=kendo.toString(replaceString(Name))#' title='#=ProcessMessage#' style='float: right; font-size: 16px; color: grey; cursor: pointer;'></div> # } else" +
                          " { # <span id='tableName_#=kendo.toString(replaceString(Name))#' style='float: left; color: orange;'>#=Name#</span><div id='tooltip_#=kendo.toString(replaceString(Name))#'></div>  # } #",
                width: 65
            }, {
                field: "SnowCount",
                template:
                    "#if (UsedInOtherSync == false) {# <div id='snowcount_#=kendo.toString(replaceString(Name))#'><span>#=SnowCount#</span></div> #} else" +
                        " { #  # } #",
                title: "SnowCount",
                filterable: false,
                width: 20,
                hidden: hideColSnowCount
            }, {
                field: "RowCount",
                template:
                    "#if (UsedInOtherSync == false) {# <div id='maxcount_#=kendo.toString(replaceString(Name))#'><span>#=RowCount#</span></div> #} else" +
                        " { #  # } #",
                title: "Found",
                filterable: false,
                width: 20,
                hidden: hideColRecordCount
            }, {
                field: "SqlCount",
                template: "#if (UsedInOtherSync == false) {# <div id='sqlcount_#=kendo.toString(replaceString(Name))#'><span>#=SqlCount#</span></div> #} else" +
                    " { #  # } #",
                title: "SqlCount",
                filterable: false,
                width: 20,
                hidden: hideColSqlCount
            }, {
                field: "Progress",
                template: "#if (UsedInOtherSync == false) {# <div class='progress' id='prg_#=kendo.toString(replaceString(Name))#'><span></span></div> #} else if" + 
                          " (UsedInOtherSync == true) { # <span>... synchronized in Core-Sync ...</span> # } #",
                width: 45,
                filterable: false,
                editable: false
            }, {
                field: "Inserted",
                template: "#if (UsedInOtherSync == false) {# <div id='inserted_#=kendo.toString(replaceString(Name))#'><span>#=Inserted#</span></div> #} else" +
                          " { #  # } #",
                title: "Inserted",
                width: 15,
                filterable: false,
                editable: false,
                hidden: hideColInserted
            }, {
                field: "Updated",
                template: "#if (UsedInOtherSync == false) {# <div id='updated_#=kendo.toString(replaceString(Name))#'><span>#=Updated#</span></div> #} else" +
                          " { #  # } #",
                title: colUpdatedName,
                width: 15,
                filterable: false,
                editable: false
            }, {
                field: "Failures",
                template: "#if (UsedInOtherSync == false) {# <div id='failures_#=kendo.toString(replaceString(Name))#'><span>#=Failures#</span></div> #} else" +
                          " { #  # } #",
                title: "Failures",
                width: 13,
                filterable: false,
                editable: false
            }, {
                field: "Duration",
                template: "#if (UsedInOtherSync == false) {# <div id='duration_#=kendo.toString(replaceString(Name))#'><span>#=(Duration == null) ? ' ' : Duration #</span></div> #} else" +
                          " { #  # } #",
                title: "Duration",
                width: 15,
                filterable: false,
                editable: false
            }, {
                field: "StartTime",
                template: "#if (UsedInOtherSync == false) {# <div id='starttime_#=kendo.toString(replaceString(Name))#'><span>#=(StartTime == null) ? ' ' : StartTime #</span></div> #} else" +
                    " { #  # } #",
                title: "Last Start",
                width: 38,
                filterable: false,
                editable: false
            }
        ]
    }).data("kendoGrid");

    syncQueueGrid = $("#syncQueueGrid").data("kendoGrid");
    syncQueueGrid.bind("dataBound", syncQueueGridGridDataBound);
    syncQueueGrid.dataSource.fetch();
}

//Set active/inactive filter forecolor 
function syncQueueGridGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembers(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }

    var grid = this;
    grid.tbody.find(".progress").each(function (e) {
        var row = $(this).closest("tr");
        var model = grid.dataItem(row);
        
        $(this).kendoProgressBar({
            min: 0,
            max: model.RowCount,
            value: model.Progress,
            type: "value"
        });

        
    });
    
    RestartGridInterval();
}

//init filter
function initFilterMenuTableSync(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        //reset filterselection
        syncQueue = [];
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

    $('button[type="reset"]').click(function (ev) {
        //reset filterselection
        syncQueue = [];
        $("#headerInstall-chb").prop("checked", false);
    });
}

function getFieldType(dataSource, field) {
    return dataSource.options.schema.model.fields[field].type;
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
function SelectUnselectAllFlag() {
    var headerChecked = $("#headerQueue-chb").prop("checked");
    var dataSource = $("#syncQueueGrid").data("kendoGrid").dataSource;
    var filters = dataSource.filter();
    var allData = dataSource.data();
    var query = new kendo.data.Query(allData);
    var data = query.filter(filters).data;

    kendo.ui.progress($("#syncQueueGrid"), true);

    $.each(data, function (i, row) {
        var selTable = new Object();
        selTable.Name = row.Name;
        
        if (headerChecked) {
            $("#queueTable-chb_" + row.Name).prop("checked", true);
            if (syncQueue.findIndex(x => x.Name === row.Name) < 0) {
                syncQueue.push(selTable);
            }
        } else {
            $("#queueTable-chb_" + row.Name).prop("checked", false);
            if (syncQueue.findIndex(x => x.Name === row.Name) >= 0) {
                syncQueue = jQuery.grep(syncQueue, function (value) {
                    return value.Name !== row.Name;
                });
            }
        }
    });

    if (syncQueue.length > 0) {
        $("#retrySelectedTables").css("display", "block");
    } else {
        $("#retrySelectedTables").css("display", "none");
    }

    kendo.ui.progress($("#syncQueueGrid"), false);
}

//grid checkbox selection
function SelectUnselectTable(object) {

    var selItem = object[0];
    kendo.ui.progress($("#syncQueueGrid"), true);

    var changedSelection = selItem.checked, row = selItem.closest("tr"),
        grid = $("#syncQueueGrid").data("kendoGrid"),
        tableItem = grid.dataItem(row);

    if (tableItem !== null && tableItem !== undefined) {
        var selTable = new Object();

        if (selItem.checked) {
            //add if id is not in list  
            if (syncQueue.findIndex(x => x.Name === tableItem.Name) < 0) {
                selTable.Name = tableItem.Name;
                syncQueue.push(selTable);
            }
        } else {
            //remove id if it is in list
            if (syncQueue.findIndex(x => x.Name === tableItem.Name) >= 0) {
                syncQueue = jQuery.grep(syncQueue, function (value) {
                    return value.Name !== tableItem.Name;
                });
            }
        }
    }

    if (syncQueue.length > 0) {
        $("#retrySelectedTables").css("display", "block");
    } else {
        $("#retrySelectedTables").css("display", "none");
    }

    kendo.ui.progress($("#syncQueueGrid"), false);
}

//update sync progress in interval
function UpdateSyncProgress(syncId, skip, take, filterQuery) {
    
    $.ajax({
        url: "~/../../api/SnowApi/UpdateSyncProgress",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        data: {
            synchronizationId: syncId,
            skipCount: skip,
            takeCount: take,
            filter: filterQuery
        },
        success: function (res) {
            if (res !== undefined && res !== "" && res !== null) {

                $.each(res.SnowTables, function (i, table) {

                    if (table.Name.indexOf("$") > -1) {
                        table.Name = replaceString(table.Name);
                    }
                    
                    if (table.EndTime === null && (table.RowCount === 0 || table.RowCount === null)) {
                        //As long as not RowCount is readed for each table it's necessary to refresh grid for setting progressbar event
                        var dataSource = $("#syncQueueGrid").data("kendoGrid").dataSource;
                        dataSource.read();
                    }

                    if (table.Enabled) {
                        $("#tableName_" + table.Name).css({ 'color': 'grey' });
                    } else {
                        $("#tableName_" + table.Name).css({ 'color': 'silver' });
                    }

                    $("#maxcount_" + table.Name).text(table.RowCount);
                    $("#sqlcount_" + table.Name).text(table.SqlCount);

                    if ($("#prg_" + table.Name).data("kendoProgressBar") !== undefined && $("#prg_" + table.Name).data("kendoProgressBar") !== null) {

                        $("#prg_" + table.Name).data("kendoProgressBar").value(table.Progress);
                    }

                    if (table.Enabled) {
                        $("#rerun_" + table.Name).css({ 'color': 'brown' });
                    } else {
                        $("#rerun_" + table.Name).css({ 'color': 'silver' });
                    }

                    $("#inserted_" + table.Name).text(table.Inserted);
                    $("#updated_" + table.Name).text(table.Updated);
                    $("#deleted_" + table.Name).text(table.Deleted);
                    $("#failures_" + table.Name).text(table.Failures);
                    if (table.Duration !== null) {
                        $("#duration_" + table.Name).text(table.Duration);
                    } else {
                        $("#duration_" + table.Name).text("");
                    }
                    $("#starttime_" + table.Name).text(table.StartTime);
                    $("#tooltip_" + table.Name).prop('title', table.ProcessMessage);

                    var elementSyncState = $("#syncState_" + table.Name);
                    var oldClass = elementSyncState.attr("class");

                    var elementSuspend = $("#suspend_" + table.Name);
                    var oldSuspendClass = elementSuspend.attr("class");

                    var elementReRun = $("#rerun_" + table.Name);
                    var oldReRunClass = elementReRun.attr("class");

                    switch (table.SyncState) {
                        case 1:
                            elementSyncState.removeClass(oldClass).addClass('fa-play-circle');
                            elementSyncState.css({ 'color': 'darkblue', 'font-size': '16px' });
                            $("#queueTable-chb_" + table.Name).prop('disabled', false);
                            break;
                        case 2:
                            elementSyncState.removeClass(oldClass).addClass('fa fa-stop-circle');
                            elementReRun.removeClass(oldReRunClass);
                            elementSyncState.css({ 'color': 'red', 'font-size': '16px' });
                            $("#queueTable-chb_" + table.Name).prop('disabled', true);

                            //suspend-continue
                            elementSuspend.removeClass(oldSuspendClass).addClass('fas fa-pause-circle');
                            elementSuspend.prop('title', 'Suspend - continue is possible');
                            elementSuspend.css({ 'color': 'red', 'font-size': '15px' });
                            elementSuspend.on("click", function () {
                                SuspendOrContinueProcess($(this), table.SyncState);
                            });
                            break;
                        case 3:
                            elementSyncState.removeClass(oldClass).addClass('fas fa-check-circle');
                            elementSyncState.css({ 'color': 'green', 'font-size': '16px' });
                            $("#queueTable-chb_" + table.Name).prop('disabled', false);
                            elementSuspend.removeClass(oldSuspendClass);
                            elementReRun.addClass('fas fa-cog');
                            elementReRun.css({ 'color': 'brown', 'font-size': '16px', 'cursor': 'pointer' });
                            elementReRun.on("click", function () {
                                SetReRunParams($(this));
                            });
                            break;
                        case 4:
                            elementSyncState.removeClass(oldClass).addClass('fas fa-trash-alt');
                            elementSyncState.css({ 'color': 'darkorange', 'font-size': '16px' });
                            elementSyncState.prop('title', 'Cleanup process');
                            $("#queueTable-chb_" + table.Name).prop('disabled', false);
                            elementSyncState.on("click", function () {
                                CleanUpProcess($(this), table.SyncState);
                            });
                            break;
                        case 5:
                            elementSyncState.removeClass(oldClass).addClass('fas fa-exclamation-circle');
                            elementSyncState.css({ 'color': 'red', 'font-size': '16px' });
                            $("#queueTable-chb_" + table.Name).prop('disabled', false);
                            break;
                        case 6:
                            elementSyncState.removeClass(oldClass).addClass('fas fa-frown');
                            elementSyncState.css({ 'color': 'red', 'font-size': '16px' });
                            $("#queueTable-chb_" + table.Name).prop('disabled', false);
                            elementSyncState.on("click", function () {
                                CleanUpProcess($(this), table.SyncState);
                            });
                            break;
                        case 7:
                            //suspend-continue
                            elementSuspend.removeClass(oldSuspendClass).addClass('fas fa-play-circle');
                            elementSuspend.prop('title', 'Continue');
                            elementSuspend.css({ 'color': 'green', 'font-size': '16px' });
                            elementSuspend.on("click", function () {
                                SuspendOrContinueProcess($(this), table.SyncState);
                            });
                            break;
                        default:
                    }

                    //console.log("ProgressBar: " + $("#prg_" + table.Name).data("kendoProgressBar") + ", Progress: " + table.Progress + ", syncState: " + table.SyncState + ", MaxCount: " + table.RowCount + ", Tooltip: " + table.ProcessMessage + ", SysId: " + table.SysId + ", TableName: " + table.Name);
                });
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alert(err.Message);
        }
    });
}

//Cleanup process - only if syncstate is interrupted
function CleanUpProcess(object, syncState) {
    var table = object[0];

    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('syncState_', '');
        $.ajax({
            url: "~/../../api/SnowApi/CleanUpProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { tableName: tableName, syncId: gbSelSyncId, syncState: syncState },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //Todo 
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alert(err.Message);
            }
        });
    }
}

//Suspend or Continue process - depend on syncstate
function SuspendOrContinueProcess(object, syncState) {
    var table = object[0];

    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('suspend_', '');
        $.ajax({
            url: "~/../../api/SnowApi/SuspendOrContinueProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { tableName: tableName, syncId: gbSelSyncId, syncState: syncState },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //Todo 
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alert(err.Message);
            }
        });
    }
}

//interrupt running process
function StopSyncProcess(object) {
    var table = object[0];
    
    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('syncState_', ''); 
        $.ajax({
            url: "~/../../api/SnowApi/StopSyncProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { tableName: tableName, syncId: gbSelSyncId },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //Todo 
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alert(err.Message);
            }
        });
    }
}

//Suspend Process
function SuspendSyncProcess(object) {
    var table = object[0];

    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('suspend_', '');
        $.ajax({
            url: "~/../../api/SnowApi/SuspendSyncProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { tableName: tableName, syncId: gbSelSyncId },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //Todo 
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alert(err.Message);
            }
        });
    }
}

//Continue Process
function ContinueSyncProcess(object) {
    var table = object[0];
    
    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('suspend_', '');
        $.ajax({
            url: "~/../../api/SnowApi/ContinueSyncProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { tableName: tableName, syncId: gbSelSyncId },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                    //Todo 
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alert(err.Message);
            }
        });
    }
}

function replaceString(value) {
    return value.replace("$", "_");
}

//set params for single table
function SetReRunParams(object) {
    var table = object[0];

    if (table !== undefined && table !== null && table.id !== undefined) {
        var tableName = table.id.replace('rerun_', '');

        $.ajax({
            url: "~/../../api/SnowApi/LoadSyncProcessParams",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { synchronizationId: gbSelSyncId, tableName: tableName },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {
                    $("#rerunEnabled").bootstrapSwitch('state', res.Enabled);
                    $("#rerunThreadsPerTable").val(res.ThreadsPerTable);
                    $("#rerunThreadSleepTime").val(res.ThreadSleepTime);
                    $("#rerunPageSize").val(res.PageSize);
                    $("#rerunRequestTimeout").val(res.RequestTimeout);
                    $("#rerunTableName").text(tableName);

                    if (res.TableInheritanceEnabled === true) {
                        $("#paramEnableTableInheritance").css('display', 'block');
                    } else {
                        $("#paramEnableTableInheritance").css('display', 'none');
                    }
                    $("#rerunTableInheritance").bootstrapSwitch('state', res.TableInheritance);

                    if (res.IsDelta === true) {
                        $("#paramCustomDeltaStart").css('display', 'block');
                        $("#rerunCustomDeltaStart").val(res.CustomDeltaStart);
                    } else {
                        $("#paramCustomDeltaStart").css('display', 'none');
                    }

                    $("#kModalReRunSettings").kendoWindow({
                        width: "600",
                        minHeight: "450",
                        title: "Set Re-Run params",
                        visible: false,
                        resizable: false,
                        actions: [
                            "Pin",
                            "Minimize",
                            "Close"
                        ],
                        close: function (e) {

                        }
                    }).data("kendoWindow").center().open();
                }
            },
            error: function (xhr, status) {
                var err = JSON.parse(xhr.responseText);
                alertify.error(err.Message);
            }
        });
    }
}

//set custom delta time
function SetCustomDelta() {

    var syncId = $("#syncQueueListTargetSqlDb option:selected").val();
    var syncName = "";

    if (syncId == undefined || syncId === "") {
        syncName = $("#syncQueueListTargetKafka option:selected").text();
    } else {
        syncName = $("#syncQueueListTargetSqlDb option:selected").text();
    }

    $("#customDeltaSyncId").text(syncId);
    $("#customDeltaSyncName").text(syncName);

    $.ajax({
        url: "~/../../api/SnowApi/LoadCustomDeltaStart",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        data: { synchronizationId: syncId },
        success: function (res) {
            if (res !== undefined && res !== "" && res !== null) {

                $("#paramSetCustomDeltaStart").val(res.CustomDeltaTime);

                $("#kModalSetCustomDelta").kendoWindow({
                    width: "550",
                    minHeight: "300",
                    title: "Set custom start date",
                    resizable: false,
                    visible: false,
                    actions: [
                        "Minimize",
                        "Close"
                    ],
                    close: function (e) {

                    }
                }).data("kendoWindow").center().open();
            }
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
        
}

//update customdelta for synchronization
function UpdateSetCustomDelta() {

    var syncId = $("#syncQueueListTargetSqlDb option:selected").val();
    
    if (syncId == undefined || syncId === "") {
        syncId = $("#syncQueueListTargetKafka option:selected").val()
    } 

    var syncObject = new Object();
    syncObject.SynchronizationId = syncId;
    syncObject.SynchronizationName = $("#customDeltaSyncName").text();
    syncObject.CustomDeltaTime = $("#paramSetCustomDeltaStart").val();

    $.ajax({
        url: "~/../../api/SnowApi/SetCustomDeltaStart",
        dataType: "json",
        contentType: "application/json",
        type: 'POST',
        data: JSON.stringify(syncObject),
        success: function (res) {
            alertify.success('Parameters saved');
            $("#kModalSetCustomDelta").data("kendoWindow").close();
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });

    
}

//post new syncprocess single table parameters
function UpdateSyncProcessParams() {
    var retryObject = new Object();
    retryObject.SynchronizationId = gbSelSyncId;
    retryObject.Enabled = $('#rerunEnabled').bootstrapSwitch('state');
    retryObject.TableName = $("#rerunTableName").text();
    retryObject.ThreadsPerTable = $("#rerunThreadsPerTable").val();
    retryObject.ThreadSleepTime = $("#rerunThreadSleepTime").val();
    retryObject.PageSize = $("#rerunPageSize").val();
    retryObject.RequestTimeout = $("#rerunRequestTimeout").val();
    retryObject.TableInheritance = $('#rerunTableInheritance').bootstrapSwitch('state');
    retryObject.CustomDeltaStart = $("#rerunCustomDeltaStart").val();

    $.ajax({
        url: "~/../../api/SnowApi/UpdateSyncProcessParams",
        dataType: "json",
        contentType: "application/json",
        type: 'POST',
        data: JSON.stringify(retryObject),
        success: function (res) {
            alertify.success('Parameters saved');
            $("#kModalReRunSettings").data("kendoWindow").close();
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}

//start retry synchronization from selected tables
function RetrySync() {
    if (syncQueue.length > 0) {

        var syncId = $("#syncQueueListTargetSqlDb option:selected").val();

        if (syncId == undefined || syncId === "") {
            syncId = $("#syncQueueListTargetKafka option:selected").val();
        }
        
        var retryObject = new Object();
        retryObject.SynchronizationId = syncId;
        retryObject.SnowTables = syncQueue;
        
        $.ajax({
            url: "~/../../api/SnowApi/RetrySyncProcess",
            dataType: "json",
            contentType: "application/json",
            type: 'POST',
            data: JSON.stringify(retryObject),
            success: function (res) {
                syncQueue.forEach(function (item) {
                    $("#queueTable-chb_" + item.Name).prop('disabled', true);
                    $("#queueTable-chb_" + item.Name).prop("checked", false);
                    if (syncQueue.findIndex(x => x.Name === item.Name) >= 0) {
                            syncQueue = jQuery.grep(syncQueue, function (value) {
                                return value.Name !== item.Name;
                            });
                    }
                });
                $("#retrySelectedTables").css("display", "none");
            },
            error: function (xhr, status) {
                var err = JSON.parse(xhr.responseText);
                alertify.error(err.Message);
            }
        });
    }
}

//restart interval
function RestartGridInterval() {
    clearInterval(intervalId);

    var grid = $("#syncQueueGrid").data('kendoGrid');
    var currentPage = grid.dataSource.page();
    var currentPageSize = grid.dataSource.pageSize();
    var skip = (currentPage * currentPageSize) - currentPageSize;

    intervalId = window.setInterval(function () {
        UpdateSyncProgress(gbSelSyncId, skip, currentPageSize, filterQuery);
    }, 2500);
}

function EditCustomDeltaStart() {
    var syncId = "";

    syncId = $("#syncQueueListTargetSqlDb option:selected").val();

    if (syncId == undefined || syncId === "") {
        syncId = $("#syncQueueListTargetKafka option:selected").val();
    }

    if (syncId !== "" && syncId != undefined) {
        window.location.href = "../Manage/SyncScheduler?SyncId=" + syncId;
    }
}

//get configured grid column settings
function GetColVisibility() {

    var gridSettings = null;

    $.ajax({
        url: "~/../../api/SnowApi/GetColVisibilities",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        async: false,
        success: function (gridConfig) {
            gridSettings = gridConfig;
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });

    return gridSettings;
}