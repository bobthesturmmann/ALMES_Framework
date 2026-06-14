$(document).ready(function () {
    let currentLineCount = 0;
    let targetSelection = "main";
    let modalRawData = [];
    let modalCurrentPage = 1;

    const urlParams = new URLSearchParams(window.location.search);
    const paramProductCode = urlParams.get('productCode');
    const paramMode = urlParams.get('mode');

    // Edit veya View modunda seçici elementleri devre dışı bırakıyoruz
    if (paramMode === "edit" || paramMode === "view") {
        $("#btnSelectMainProduct").prop("disabled", true);
        $("#txtMainProductCode").prop("disabled", true);

        $("#txtMainProductCode").off("click");
        $("#btnSelectMainProduct").off("click");
    }

    if (paramProductCode) {
        $("#txtMainProductCode").val("Logo'dan yükleniyor (" + paramProductCode + ")...");

        $.ajax({
            url: "/Bom/RecipeManagement/SearchProducts",
            type: "GET",
            data: { searchCode: paramProductCode, selectionType: "main" },
            success: function (res) {
                if (res.isSuccess && res.data && res.data.length > 0) {
                    let item = res.data.find(x => x.productCode === paramProductCode) || res.data[0];

                    $("#txtMainProductCode").val(item.productCode + " - " + item.productName).attr("data-purecode", item.productCode);
                    $("#hdnMainProductRef").val(item.productRef);
                    $("#txtMainUnit").val(item.unit);
                    $("#hdnMainUnitRef").val(item.unitRef);

                    let mainQty = (item.quantity && parseFloat(item.quantity) > 0) ? parseFloat(item.quantity) : 1;
                    $("#numMainQuantity").val(mainQty);

                    if (paramMode !== "view") {
                        $("#btnAddSubProduct").prop("disabled", false);
                        $("#btnSaveAllChanges").prop("disabled", false);
                    }

                    loadExistingRecipeLines(item.productRef, item.productCode, paramMode);
                } else {
                    $("#txtMainProductCode").val("Malzeme kartı bulunamadı!");
                }
            }
        });
    }

    function loadExistingRecipeLines(ref, code, mode) {
        $("#trEmptyRow td").html('<div class="spinner-border spinner-border-sm text-primary me-2"></div> Mevcut reçete satırları Logo\'dan getiriliyor...');

        $.ajax({
            url: '/Bom/RecipeManagement/GetRecipeLines',
            type: 'GET',
            data: { mainProductCode: code },
            success: function (res) {
                if (res.isSuccess && res.data && res.data.length > 0) {
                    $("#tblRecipeLines tbody").find("tr:not(#trEmptyRow)").remove();
                    $("#trEmptyRow").hide();
                    currentLineCount = 0;

                    res.data.forEach(function (line) {
                        currentLineCount++;
                        let uniqueId = "row_" + currentLineCount;
                        let isView = (mode === "view");

                        let badgeColor = "bg-secondary";
                        if (line.subProductType === "MM") badgeColor = "bg-success";
                        else if (line.subProductType === "YM") badgeColor = "bg-warning text-dark";
                        else if (line.subProductType === "HM") badgeColor = "bg-info text-dark";
                        else if (line.subProductType === "TM") badgeColor = "bg-primary";

                        let row = `<tr id="${uniqueId}" data-db-status="original" data-subref="${line.altUrunRef}" data-subunitref="${line.altBirimRef}">
                            <td class="fw-bold text-dark">${line.subProductCode}</td>
                            <td class="text-center"><span class="badge ${badgeColor}">${line.subProductType || '--'}</span></td>
                            <td class="text-secondary fw-semibold">${line.subProductName}</td>
                            <td>
                                <input type="number" class="form-control form-control-sm txt-sub-qty" value="${parseFloat(line.subQuantity)}" style="max-width:120px;" ${isView ? "disabled" : ""}>
                            </td>
                            <td class="text-center"><span class="badge bg-light text-dark border">${line.subUnit}</span></td>`;

                        if (!isView) {
                            row += `<td class="text-center">
                                <div class="btn-group btn-group-sm">
                                    <button type="button" class="btn btn-outline-secondary btn-move-up"><i class="bi bi-chevron-up"></i></button>
                                    <button type="button" class="btn btn-outline-secondary btn-move-down"><i class="bi bi-chevron-down"></i></button>
                                    <button type="button" class="btn btn-outline-danger btn-delete-line"><i class="bi bi-trash"></i></button>
                                </div>
                            </td>`;
                        }

                        row += `</tr>`;
                        $("#tblRecipeLines tbody").append(row);
                    });
                } else {
                    let colSpanCount = (mode === "view") ? 5 : 6;
                    $("#trEmptyRow").show().find("td").attr("colspan", colSpanCount).html('<i class="bi bi-info-circle me-1"></i> Bu ürünün aktif bir reçete satırı bulunamadı.');
                }
            },
            error: function () {
                let colSpanCount = (mode === "view") ? 5 : 6;
                $("#trEmptyRow").show().find("td").attr("colspan", colSpanCount).html('<i class="bi bi-exclamation-triangle me-1"></i> Satırlar yüklenirken sistem bağlantı hatası oluştu!');
            }
        });
    }

    $(document).on("click", "#btnSelectMainProduct, #btnAddSubProduct", function () {
        let $btn = $(this);

        // GÜVENLİK KİLİDİ: Eğer buton disabled ise modalın tetiklenmesini kesin olarak engelle
        if ($btn.prop("disabled")) return;

        if ($btn.attr("id") === "btnSelectMainProduct") {
            targetSelection = "main";
            $("#productSelectModalLabel").html('<i class="bi bi-box-seam text-primary me-2"></i>Ana Ürün Seçimi');
        } else {
            targetSelection = "sub";
            $("#productSelectModalLabel").html('<i class="bi bi-layers text-success me-2"></i>Bileşen (Alt Ürün) Seçimi');
        }

        $("#modalSearchInput").val("");
        $("#modalProductTable tbody").html('<tr><td colspan="5" class="text-center text-muted py-4"><i class="bi bi-info-circle me-1"></i> Arama yapmak için yukarıdaki kutuyu kullanın.</td></tr>');

        var modalElement = document.getElementById('productSelectModal');
        var myModal = new bootstrap.Modal(modalElement);
        myModal.show();
    });

    $(document).on("click", ".btn-pick", function () {
        let code = $(this).attr("data-code");
        let name = $(this).attr("data-name");
        let unit = $(this).attr("data-unit");
        let ref = $(this).attr("data-idref");
        let unitref = $(this).attr("data-uref");
        let type = $(this).attr("data-type") || 'ALT';
        let hasRecipe = $(this).attr("data-hasrecipe") === "true";

        if (targetSelection === "main") {
            $("#txtMainProductCode").val(code + " - " + name).attr("data-purecode", code);
            $("#hdnMainProductRef").val(ref);
            $("#txtMainUnit").val(unit);
            $("#hdnMainUnitRef").val(unitref);
            $("#numMainQuantity").val("1");

            $("#btnAddSubProduct").prop("disabled", false);
            $("#btnSaveAllChanges").prop("disabled", false);

            if (hasRecipe) {
                loadExistingRecipeLines(ref, code, "edit");
            } else {
                $("#tblRecipeLines tbody").find("tr:not(#trEmptyRow)").remove();
                $("#trEmptyRow").show().find("td").attr("colspan", 6).html('<i class="bi bi-info-circle me-1"></i> Bu ürünün henüz bir reçetesi yok. Sağ üstten <b>Bileşen Ekle</b> diyerek yeni reçete oluşturabilirsiniz.');
            }
        }
        else if (targetSelection === "sub") {
            let isAlreadyExists = false;
            $("#tblRecipeLines tbody tr").not("#trEmptyRow").each(function () {
                let existingCode = $(this).find("td:eq(0)").text().trim();
                let dbStatus = $(this).attr("data-db-status");
                if (existingCode === code && dbStatus !== "deleted") { isAlreadyExists = true; return false; }
            });

            if (isAlreadyExists) { alert("Bu bileşen (`" + code + "`) tabloda zaten mevcut!"); return; }

            $("#trEmptyRow").hide();
            currentLineCount++;

            let badgeColor = "bg-secondary";
            if (type === "MM") badgeColor = "bg-success";
            else if (type === "YM") badgeColor = "bg-warning text-dark";
            else if (type === "HM") badgeColor = "bg-info text-dark";
            else if (type === "TM") badgeColor = "bg-primary";

            let uniqueId = "row_" + currentLineCount;

            let newRow = `<tr id="${uniqueId}" data-db-status="new" data-subref="${ref}" data-subunitref="${unitref}">
                <td class="fw-bold text-dark">${code}</td>
                <td class="text-center"><span class="badge ${badgeColor}">${type}</span></td>
                <td class="text-secondary fw-semibold">${name}</td>
                <td>
                    <input type="number" class="form-control form-control-sm txt-sub-qty" value="1" step="1" min="0" style="max-width:120px;">
                </td>
                <td class="text-center"><span class="badge bg-light text-dark border">${unit}</span></td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-secondary btn-move-up"><i class="bi bi-chevron-up"></i></button>
                        <button type="button" class="btn btn-outline-secondary btn-move-down"><i class="bi bi-chevron-down"></i></button>
                        <button type="button" class="btn btn-outline-danger btn-delete-line"><i class="bi bi-trash"></i></button>
                    </div>
                </td>
            </tr>`;

            $("#tblRecipeLines tbody").append(newRow);
        }

        var modalElement = document.getElementById('productSelectModal');
        var modalInstance = bootstrap.Modal.getInstance(modalElement);
        if (modalInstance) { modalInstance.hide(); }
    });

    $(document).on("click", ".btn-delete-line", function () {
        let $tr = $(this).closest("tr");
        let dbStatus = $tr.attr("data-db-status");
        if (dbStatus === "new") { $tr.remove(); } else { $tr.attr("data-db-status", "deleted").hide(); }
        checkTableEmptyState();
    });

    function checkTableEmptyState() {
        let visibleRows = $("#tblRecipeLines tbody tr:visible").not("#trEmptyRow").length;
        if (visibleRows === 0) {
            let colSpanCount = (paramMode === "view") ? 5 : 6;
            $("#trEmptyRow").show().find("td").attr("colspan", colSpanCount).html('<i class="bi bi-info-circle me-1"></i> Reçete satırı kalmadı.');
        } else {
            $("#trEmptyRow").hide();
        }
    }

    $("#btnClearForm").click(function () { if (confirm("Tüm formu sıfırlamak istediğinize emin misiniz?")) { location.reload(); } });

    $("#btnSaveAllChanges").on("click", function (e) {
        e.preventDefault();
        let $btn = $(this);
        let mainProductRef = parseInt($("#hdnMainProductRef").val()) || 0;
        let mainQuantity = parseFloat($("#numMainQuantity").val()) || 1.0;
        let mainUnitRef = parseInt($("#hdnMainUnitRef").val()) || 1;

        let payload = { AnaUrunRef: mainProductRef, AnaMiktar: mainQuantity, AnaBirimRef: mainUnitRef, IsDeleteAll: false, Lines: [] };

        $("#tblRecipeLines tbody tr").not("#trEmptyRow").each(function (idx) {
            let $tr = $(this);
            payload.Lines.push({
                Status: $tr.attr("data-db-status"),
                SatirNo: idx + 1,
                AltUrunRef: parseInt($tr.attr("data-subref")),
                AltBirimRef: parseInt($tr.attr("data-subunitref")),
                AltMiktar: parseFloat($tr.find(".txt-sub-qty").val()) || 0
            });
        });

        let activeCount = payload.Lines.filter(l => l.Status !== "deleted").length;
        if (activeCount === 0 && payload.Lines.length > 0) {
            if (confirm("Tüm satırları sildiniz. REÇETEYİ LOGO'DAN KOMPLE SİLMEK İSTİYOR MUSUNUZ?")) { payload.IsDeleteAll = true; } else { return; }
        }

        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm"></span> Kaydediliyor...');

        $.ajax({
            url: '/Bom/RecipeManagement/SaveBulkChanges',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.isSuccess) {
                    alert("Tüm değişiklikler başarıyla Logo'ya işlendi!");
                    window.location.href = "/Bom/RecipeList";
                } else {
                    alert("Hata: " + res.message);
                    $btn.prop("disabled", false).html('<i class="bi bi-cloud-check-fill me-2"></i> Tüm Değişiklikleri Kaydet');
                }
            }
        });
    });

    function fetchModalProducts() {
        let searchKeyword = $("#modalSearchInput").val().trim();
        $.ajax({
            url: "/Bom/RecipeManagement/SearchProducts",
            type: "GET",
            data: { searchCode: searchKeyword, selectionType: targetSelection },
            success: function (res) {
                if (res.isSuccess && res.data) {
                    modalRawData = res.data;
                    modalCurrentPage = 1;
                    renderModalPagedTable();
                }
            }
        });
    }

    function renderModalPagedTable() {
        let $tbody = $("#modalProductTable tbody");
        $tbody.empty();

        if (!modalRawData || modalRawData.length === 0) {
            $tbody.html('<tr><td colspan="5" class="text-center text-muted py-4">Ürün bulunamadı.</td></tr>');
            return;
        }

        let modalPageSize = 20;
        let pagedItems = modalRawData.slice((modalCurrentPage - 1) * modalPageSize, modalCurrentPage * modalPageSize);

        pagedItems.forEach(function (item) {
            let badgeColor = "bg-secondary";
            if (item.productType === "MM") badgeColor = "bg-success";
            else if (item.productType === "YM") badgeColor = "bg-warning text-dark";
            else if (item.productType === "HM") badgeColor = "bg-info text-dark";
            else if (item.productType === "TM") badgeColor = "bg-primary";

            let row = `<tr>
                <td class="fw-bold text-dark">${item.productCode}</td>
                <td class="text-center"><span class="badge ${badgeColor}">${item.productType || '--'}</span></td>
                <td class="text-secondary">${item.productName}</td>
                <td class="text-center"><span class="badge bg-light text-dark border">${item.unit}</span></td>
                <td class="text-center">
                    <button class="btn btn-sm btn-success btn-pick" 
                        data-code="${item.productCode}" 
                        data-name="${item.productName}" 
                        data-unit="${item.unit}" 
                        data-idref="${item.productRef}" 
                        data-uref="${item.unitRef}" 
                        data-type="${item.productType || ''}" 
                        data-hasrecipe="${item.isRecipeExists}">
                        <i class="bi bi-check2"></i> Seç
                    </button>
                </td>
            </tr>`;
            $tbody.append(row);
        });

        updatePaginationUI(Math.ceil(modalRawData.length / modalPageSize));
    }

    function updatePaginationUI(maxPage) {
        if (maxPage <= 1) { $("#modalPaginationWrapper").hide(); } else { $("#modalPaginationWrapper").attr("style", "display: flex !important;"); }
        $("#manualModalPageInput").val(modalCurrentPage).attr("max", maxPage);
        $("#lblModalMaxPage").text("/ " + maxPage);
        $("#btnModalPrevPage").prop("disabled", modalCurrentPage <= 1);
        $("#btnModalNextPage").prop("disabled", modalCurrentPage >= maxPage);
    }

    $(document).on("click", "#btnModalSearch", function () { fetchModalProducts(); });
    $(document).on("keypress", "#modalSearchInput", function (e) { if (e.which == 13) { fetchModalProducts(); } });
    $(document).on("click", "#btnModalJump", function () {
        let input = document.getElementById('manualModalPageInput');
        let pageNo = parseInt(input.value);
        let maxPage = parseInt(input.getAttribute('max')) || 1;
        if (!isNaN(pageNo) && pageNo >= 1 && pageNo <= maxPage) {
            modalCurrentPage = pageNo;
            renderModalPagedTable();
        } else {
            alert('Geçersiz sayfa!');
            input.value = modalCurrentPage;
        }
    });
    $(document).on("click", "#btnModalPrevPage", function () {
        if (modalCurrentPage > 1) { modalCurrentPage--; renderModalPagedTable(); }
    });
    $(document).on("click", "#btnModalNextPage", function () {
        let maxPage = Math.ceil(modalRawData.length / 20);
        if (modalCurrentPage < maxPage) { modalCurrentPage++; renderModalPagedTable(); }
    });

    $(document).on("click", ".btn-move-up", function () {
        let $row = $(this).closest("tr");
        if ($row.prev().not("#trEmptyRow").length) {
            $row.insertBefore($row.prev());
            reIndexTableRows();
        }
    });

    $(document).on("click", ".btn-move-down", function () {
        let $row = $(this).closest("tr");
        if ($row.next().length) {
            $row.insertAfter($row.next());
            reIndexTableRows();
        }
    });

    function reIndexTableRows() {
        let index = 1;
        $("#tblRecipeLines tbody tr:visible").not("#trEmptyRow").each(function () {
            $(this).attr("data-satirno", index);
            index++;
        });
    }
});