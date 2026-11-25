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
    aviso: (titulo, mensaje) => Swal.fire({
        title: titulo,
        text: mensaje,
        icon: "success",
        confirmButtonText: "Entendido",
        customClass: { popup: "autobrillo-swal", confirmButton: "autobrillo-swal-principal" }
    }),
    pedirTexto: async (titulo, mensaje) => {
        const resultado = await Swal.fire({
            title: titulo,
            text: mensaje,
            input: "textarea",
            inputPlaceholder: "Escribí el motivo",
            showCancelButton: true,
            confirmButtonText: "Confirmar",
            cancelButtonText: "Cancelar",
            reverseButtons: true,
            focusCancel: true,
            customClass: { popup: "autobrillo-swal", confirmButton: "autobrillo-swal-principal", cancelButton: "autobrillo-swal-cancelar" },
            inputValidator: (valor) => (!valor || valor.trim().length < 3 ? "Indicá el motivo (al menos 3 caracteres)." : undefined)
        });
        if (!resultado.isConfirmed) return null;
        const texto = (resultado.value || "").trim();
        return texto.length === 0 ? null : texto;
    },
    error: (mensaje) => Swal.fire({
        title: "No se pudo eliminar",
        text: mensaje,
        icon: "error",
        confirmButtonText: "Entendido",
        customClass: { popup: "autobrillo-swal", confirmButton: "autobrillo-swal-principal" }
    })
};
