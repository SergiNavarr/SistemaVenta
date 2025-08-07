// Modelo base de usuario, usado como plantilla para crear o editar
const MODELO_BASE = {
    idUsuario: 0,
    nombre: '',
    correo: '',
    telefono: '',
    idRol: 0,
    esActivo: 1,
    urlFoto: '',
}

let tablaData; // Variable global para acceder a la tabla

$(document).ready(function () {

    // Obtener roles desde el servidor y agregarlos al combo cboRol
    fetch("/Usuario/ListaRoles")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response)
        })
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    // Agrega cada rol como una opción en el combo
                    $("#cboRol").append(
                        $("<option>").val(item.idRol).text(item.descripcion)
                    )
                })
            }
        })

    // Inicializa el DataTable con configuración y carga de datos
    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Usuario/Lista', // URL para obtener los usuarios
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idUsuario", "visible": false, "searchable": false }, // ID oculto
            {
                // Muestra imagen del usuario
                "data": "urlFoto", render: function (data) {
                    return `<img style="height:60px" src=${data} class="rounded mx-auto d-block" />`;
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
            {
                // Muestra el estado como texto con estilo
                "data": "esActivo", render: function (data) {
                    return data == 1
                        ? '<span class="badge badge-info">Activo</span>'
                        : '<span class="badge badge-danger">No activo</span>';
                }
            },
            {
                // Botones de editar y eliminar
                "defaultContent": `
                    <button class="btn btn-primary btn-editar btn-sm mr-2">
                        <i class="fas fa-pencil-alt"></i>
                    </button>
                    <button class="btn btn-danger btn-eliminar btn-sm">
                        <i class="fas fa-trash-alt"></i>
                    </button>`,
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]], // Ordena por ID descendente
        dom: "Bfrtip", // Botones y filtros
        buttons: [
            {
                // Botón para exportar a Excel
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6] // Columnas visibles a exportar
                }
            },
            'pageLength' // Selector de cantidad de filas por página
        ],
        language: {
            // Traducción al español
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});


// Muestra el modal de crear/editar usuario con los datos del modelo
function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idUsuario)
    $("#txtNombre").val(modelo.nombre)
    $("#txtCorreo").val(modelo.correo)
    $("#txtTelefono").val(modelo.telefono)
    $("#cboRol").val(modelo.idRol == 0 ? $("#cboRol option:first").val() : modelo.idRol)
    $("#cboEstado").val(modelo.esActivo)
    $("#txtFoto").val("") // Limpia el input de foto
    $("#imgUsuario").attr("src", modelo.urlFoto)

    $("#modalData").modal("show") // Muestra el modal
}

// Botón "Nuevo usuario" abre el modal vacío
$("#btnNuevo").click(function () {
    mostrarModal();
})


// Evento al hacer clic en el botón "Guardar"
$("#btnGuardar").click(function () {

    // Validación: Recorre los inputs con clase .input-validar y revisa si hay alguno vacío
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    // Si hay al menos un input vacío, muestra mensaje y enfoca en él
    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return; // Cancela la ejecución
    }

    // Clona el modelo base para no modificar el original
    const modelo = structuredClone(MODELO_BASE);

    // Asigna valores del formulario al modelo
    modelo["idUsuario"] = parseInt($("#txtId").val());
    modelo["nombre"] = $("#txtNombre").val();
    modelo["correo"] = $("#txtCorreo").val();
    modelo["telefono"] = $("#txtTelefono").val();
    modelo["idRol"] = parseInt($("#cboRol").val());
    modelo["esActivo"] = parseInt($("#cboEstado").val());

    // Obtiene el archivo cargado (imagen)
    const inputFoto = document.getElementById("txtFoto");

    // Crea un FormData para enviar el archivo + datos del modelo
    const formData = new FormData();
    formData.append("foto", inputFoto.files[0]); // Imagen
    formData.append("modelo", JSON.stringify(modelo)); // Modelo como JSON

    // Muestra animación de carga en el modal
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    // Si es un nuevo usuario (idUsuario == 0)
    if (modelo.idUsuario == 0) {
        fetch("/Usuario/Crear", {
            method: "POST",
            body: formData
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    // Éxito: agrega usuario a la tabla
                    tablaData.row.add(responseJson.objeto).draw(false);
                    $("#modalData").modal("hide");
                    swal("Listo!", "El usuario fue creado", "success");
                } else {
                    // Error desde el backend
                    swal("Lo sentimos!", responseJson.mensaje, "error");
                }
            })
            .catch(error => {
                // Error general (conexión, servidor, etc.)
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                swal("Error", "Ocurrió un problema al guardar el usuario.", "error");
                console.error(error);
            });

    } else {
        // Si se está editando un usuario existente
        fetch("/Usuario/Editar", {
            method: "PUT",
            body: formData
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    // Éxito: actualiza fila en la tabla
                    tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                    filaSeleccionada = null;
                    $("#modalData").modal("hide");
                    swal("Listo!", "El usuario fue Modificado", "success");
                } else {
                    // Error desde el backend
                    swal("Lo sentimos!", responseJson.mensaje, "error");
                }
            })
            .catch(error => {
                // Error general
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                swal("Error", "Ocurrió un problema al editar el usuario.", "error");
                console.error(error);
            });
    }
});


// Variable global para guardar la fila seleccionada (al editar)
let filaSeleccionada;

// Evento al hacer clic en botón editar
$("#tbdata tbody").on("click", ".btn-editar", function () {
    // Si la fila es una versión "child" de DataTables, accede a la fila padre
    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    } else {
        filaSeleccionada = $(this).closest("tr");
    }

    // Obtiene los datos de esa fila y los pasa al modal
    const data = tablaData.row(filaSeleccionada).data();
    mostrarModal(data); // Abre el modal con los datos cargados
});


// Evento al hacer clic en botón eliminar
$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    let fila;

    // Igual que en editar: maneja filas "child"
    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    } else {
        fila = $(this).closest("tr");
    }

    const data = tablaData.row(fila).data();

    // Muestra confirmación con SweetAlert
    swal({
        title: "¿Está seguro de eliminar?",
        text: `Eliminar al usuario "${data.nombre}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {
            if (respuesta) {
                // Muestra animación de carga
                $(".showSweetAlert").LoadingOverlay("show");

                // Envia solicitud DELETE al backend
                fetch(`/Usuario/Eliminar?IdUsuario=${data.idUsuario}`, {
                    method: "DELETE",
                })
                    .then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide");
                        return response.ok ? response.json() : Promise.reject(response);
                    })
                    .then(responseJson => {
                        if (responseJson.estado) {
                            // Elimina fila de la tabla
                            tablaData.row(fila).remove().draw();
                            swal("Listo!", "El usuario fue Eliminado", "success");
                        } else {
                            swal("Lo sentimos!", responseJson.mensaje, "error");
                        }
                    })
                    .catch(error => {
                        $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                        swal("Error", "Ocurrió un problema al Eliminar el usuario.", "error");
                        console.error(error);
                    });
            }
        });
});
