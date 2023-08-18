var dataTable;

$(document).ready(function () {
    loadDataTable();
});

functionloadDataTable(){
    dataTable=$('#tblData').dataTable({
        "ajax": {url:'/admin/order/getall'},
    });
}