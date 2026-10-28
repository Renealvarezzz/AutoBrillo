window.autobrilloAlerta = {
    confirmarEliminar: async (titulo, nombre) => {
        const resultado = await Swal.fire({
            title: titulo,
            text: `Se eliminará el usuario ${nombre}.`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            reverseButtons: true,
            focusCancel: true,
            customClass: {
                popup: "autobrillo-swal",
                confirmButton: "autobrillo-swal-eliminar",
                cancelButton: "autobrillo-swal-cancelar"
            }
        });
        return resultado.isConfirmed;
    },
    confirmarEliminarRol: async (titulo, descripcion) => {
        const resultado = await Swal.fire({
            title: titulo,
            text: `Se eliminará el rol ${descripcion}. Esta acción solo es posible si ningún usuario lo utiliza.`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sí, eliminar",
            cancelButtonText: "Cancelar",
            reverseButtons: true,
            focusCancel: true
        });
        return resultado.isConfirmed;
    },
    confirmarAgregarRol: async (titulo, descripcion) => {
        const resultado = await Swal.fire({
            title: titulo,
            text: `Se agregará el rol ${descripcion}.`,
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Sí, agregar",
            cancelButtonText: "Cancelar",
            reverseButtons: true,
            focusCancel: true
        });
        return resultado.isConfirmed;
    },
    mensajeError: (titulo, mensaje) => Swal.fire({
        title: titulo,
        text: mensaje,
        icon: "error",
        confirmButtonText: "Entendido"
    }),
    exito: (mensaje) => Swal.fire({
        title: "Eliminado",
        text: mensaje,
        icon: "success",
        confirmButtonText: "Entendido",
        customClass: { popup: "autobrillo-swal", confirmButton: "autobrillo-swal-principal" }
    }),
    error: (mensaje) => Swal.fire({
        title: "No se pudo eliminar",
        text: mensaje,
        icon: "error",
        confirmButtonText: "Entendido",
        customClass: { popup: "autobrillo-swal", confirmButton: "autobrillo-swal-principal" }
    })
};
