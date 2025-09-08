$(document).ready(function () {

    InitializeDatabaseGrid();

    $("#databaseGrid").on("click", "#deleteDatabase", function (e) {
        e.preventDefault();
        var row = $(this).closest("tr");
        var grid = $("#databaseGrid").data("kendoGrid");
        var gridItem = grid.dataItem(row);
        DeleteDatabase(gridItem.Id);
    });
});


//Load Snow table-grid
function InitializeDatabaseGrid() {
    $("#databaseGrid").css("display", "block");

    $("#databaseGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 650,
        autoBind: false,
        toolbar: kendo.template($("#databaseKendoTemplate").html()),
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
        filterMenuInit: initFilterMenuDbGrid,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 30, 40, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Databases",
                itemsPerPage: "Databases per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeDatabaseGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET'
                    
                }
            },
            schema: {
                data: function (result) {
                    return result.DatabaseList;
                },
                total: function (result) {
                    return result.DatabaseListTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        Servername: { type: "string", editable: false },
                        Port: { type: "string", editable: false },
                        Instancename: { type: "string", editable: false },
                        Databasename: { type: "string", editable: false },
                        Schemaname: { type: "string", editable: false },
                        Username: { type: "string", editable: false },
                        Password: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("databaseGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("databaseGridPageSize", 10);
                }
                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true,
            pageSize: 30
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "Servername",
                title: "Server",
                width: 80,
                filterable: false,
                sortable: false
            }, {
                field: "Port",
                title: "Port",
                width: 20,
                filterable: false,
                sortable: false
            }, {
                field: "Instancename",
                title: "InstanceName",
                width: 70,
                filterable: false,
                sortable: false
            }, {
                field: "Databasename",
                title: "DatabaseName",
                width: 70,
                filterable: false,
                sortable: false
            }, {
                field: "Schemaname",
                title: "Schema",
                width: 30,
                filterable: false,
                sortable: false
            }, {
                field: "Username",
                title: "UserName",
                width: 40,
                filterable: false,
                sortable: false
            }, {
                field: "Password",
                title: "Password",
                width: 40,
                filterable: false,
                sortable: false
            }, {
                width: 40,
                template: "<button class='k-button' id='deleteDatabase'>Remove</button>",
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    var databaseGrid = $("#databaseGrid").data("kendoGrid");
    databaseGrid.bind("dataBound", databaseGridDataBound);
    databaseGrid.dataSource.fetch();
}

function databaseGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersDbGrid(filter, filteredMembers);
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
function setFilteredMembersDbGrid(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

//init filter
function initFilterMenuDbGrid(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        //reset filterselection
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
}

function AddDatabase() {
    
    var databaseObject = new Object();
    databaseObject.ServerName = $("#serverId").val();
    databaseObject.Port = $("#portId").val();
    databaseObject.Instancename = $("#instanceId").val();
    databaseObject.Databasename = $("#databaseId").val();
    databaseObject.Schemaname = $("#schemaId").val();
    databaseObject.Username = $("#usernameId").val();
    databaseObject.Password = $("#passwordId").val();

    $.ajax({
        url: "~/../../api/SnowApi/AddDatabase",
        dataType: "json",
        contentType: "application/json",
        type: 'POST',
        data: JSON.stringify(databaseObject),
        success: function (res) {
            InitializeDatabaseGrid();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });

    
}

function DeleteDatabase(gridItemId) {
    
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/RemoveDatabase",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { databaseId: gridItemId },
        success: function (res) {
            InitializeDatabaseGrid();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}