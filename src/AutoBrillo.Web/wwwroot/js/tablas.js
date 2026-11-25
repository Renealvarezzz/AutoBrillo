// AdminLTE 4 y DataTables para Blazor WebAssembly.
// Las listas se inicializan después del render y se destruyen antes de que
// Blazor vuelva a dibujar la tabla. Si no, DataTables mueve las filas y el
// diff de Blazor pierde los @onclick (editar, activar, anular, etc.).
window.abAdmin = {
    modo: function (autenticado) {
        document.body.classList.toggle("layout-fixed", autenticado);
        document.body.classList.toggle("sidebar-expand-lg", autenticado);
        document.body.classList.toggle("bg-body-tertiary", autenticado);
        document.body.classList.toggle("autobrillo-sesion", autenticado);
    },
    iniciar: function () {
        if (window.adminlte && typeof window.adminlte.initialize === "function") {
            window.adminlte.initialize();
        }
    }
};

window.abTablas = {
    idioma: {
        decimal: ",",
        thousands: ".",
        emptyTable: "No hay datos disponibles",
        info: "Mostrando _START_ a _END_ de _TOTAL_ registros",
        infoEmpty: "Mostrando 0 registros",
        infoFiltered: "(filtrado de _MAX_ registros en total)",
        lengthMenu: "Mostrar _MENU_ registros",
        loadingRecords: "Cargando...",
        processing: "Procesando...",
        search: "Buscar:",
        zeroRecords: "No se encontraron coincidencias",
        paginate: {
            first: "Primero",
            last: "Último",
            next: "Siguiente",
            previous: "Anterior"
        },
        aria: {
            sortAscending: ": activar para ordenar de forma ascendente",
            sortDescending: ": activar para ordenar de forma descendente"
        }
    },
    iniciar: function (id) {
        const tabla = document.getElementById(id);
        if (!tabla || !window.DataTable) return;
        this.destruir(id);
        const sinOrden = Array.from(tabla.querySelectorAll("thead th")).flatMap(function (th, indice) {
            const texto = (th.textContent || "").trim().toLowerCase();
            return texto === "acciones" || texto === "" ? [indice] : [];
        });
        const opciones = {
            paging: true,
            searching: true,
            ordering: true,
            info: true,
            pageLength: 10,
            lengthMenu: [10, 25, 50, 100],
            autoWidth: false,
            order: [],
            language: this.idioma
        };
        if (sinOrden.length > 0) {
            opciones.columnDefs = [{ orderable: false, targets: sinOrden }];
        }
        new DataTable(tabla, opciones);
    },
    destruir: function (id) {
        const tabla = document.getElementById(id);
        if (!tabla || !window.DataTable || !DataTable.isDataTable(tabla)) return;
        try {
            new DataTable(tabla).destroy();
        } catch (error) {
            console.warn("No se pudo destruir DataTables", id, error);
        }
    }
};
