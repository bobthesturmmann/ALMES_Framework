$(document).ready(function () {
    let currentLineCount = 0;
    let targetSelection = "main"; // main veya sub

    // 1. ANA ÜRÜN VEYA BİLEŞEN SEÇİM MODALINI AÇMA
    $(document).on("click", "#btnSelectMainProduct, #btnAddSubProduct", function () {
        let $btn = $(this);
        if ($btn.attr("id") === "btnSelectMainProduct") {
            targetSelection = "main";
            $("#productSelectModalLabel").html('<i class="bi bi-box-seam text-primary me-2"></i>Ana Ürün Seçimi');
        } else {
            targetSelection = "sub";
            $("#productSelectModalLabel").html('<i class="bi bi-layers text-success me-2"></i>Bileşen (Alt Ürün) Seçimi');
        }

        $("#modalSearchInput").val("");
        $("#modalProductTable tbody").html('<tr><td colspan="4" class="text-center text-muted py-4"><i class="bi bi-info-circle me-1"></i> Arama yapmak için yukarıdaki kutuyu kullanın.</td></tr>');

        var modalElement = document.getElementById('productSelectModal');
        var myModal = new bootstrap.Modal(modalElement);
        myModal.show();
    });

    // 2. MODAL İÇİNDE ARAMA YAPMA (AJAX)
    $(document).on("click", "#btnModalSearch", function () {
        let searchKeyword = $("#modalSearchInput").val().trim();
        let $tbody = $("#modalProductTable tbody");
        $tbody.html('<tr><td colspan="4" class="text-center py-4"><div class="spinner-border spinner-border-sm text-primary me-2"></div>Logo veritabanında aranıyor...</td></tr>');

        $.ajax({
            url: "/Bom/RecipeManagement/SearchProducts",
            type: "GET",
            data: { searchCode: searchKeyword, selectionType: targetSelection },
            success: function (res) {
                if (res.isSuccess && res.data && res.data.length > 0) {
                    $tbody.empty();
                    res.data.forEach(function (item) {
                        let hasRecipe = item.isRecipeExists === true;
                        let rowClass = (targetSelection === "main" && hasRecipe) ? "table-success text-dark" : "";
                        let badgeText = hasRecipe ? "Reçeteli" : "Reçetesiz";
                        let badgeClass = hasRecipe ? "bg-success" : "bg-secondary";

                        let pCode = item.productCode || '';
                        let pName = item.productName || '';
                        let pUnit = item.unit || '';
                        let pRef = item.productRef || 0;
                        let pUnitRef = item.unitRef || 1;

                        let row = `<tr class="${rowClass}">
                            <td class="fw-bold">
                                ${pCode} 
                                ${targetSelection === 'main' ? `<span class="badge ${badgeClass} ms-1 small" style="font-size:0.7rem;">${badgeText}</span>` : ''}
                            </td>
                            <td>${pName}</td>
                            <td><span class="badge bg-light text-dark border">${pUnit}</span></td>
                            <td class="text-center">
                                <button class="btn btn-sm btn-success btn-pick" 
                                        data-code="${pCode}" 
                                        data-name="${pName}" 
                                        data-unit="${pUnit}"
                                        data-idref="${pRef}"
                                        data-uref="${pUnitRef}"
                                        data-hasrecipe="${hasRecipe}">
                                    <i class="bi bi-check2"></i> Seç
                                </button>
                            </td>
                        </tr>`;
                        $tbody.append(row);
                    });
                } else {
                    $tbody.html('<tr><td colspan="4" class="text-center text-danger py-4"><i class="bi bi-exclamation-triangle"></i> Uygun malzeme kartı bulunamadı!</td></tr>');
                }
            },
            error: function () {
                $tbody.html('<tr><td colspan="4" class="text-center text-danger py-4">Sistem haberleşme hatası!</td></tr>');
            }
        });
    });

    $(document).on("keypress", "#modalSearchInput", function (e) {
        if (e.which == 13) { $("#btnModalSearch").click(); }
    });

    // 3. SEÇİLEN ÜRÜNÜ EKRANA BAĞLAMA VE OTOMATİK SATIR OKUMA
    $(document).on("click", ".btn-pick", function () {
        let code = $(this).attr("data-code");
        let name = $(this).attr("data-name");
        let unit = $(this).attr("data-unit");
        let ref = $(this).attr("data-idref");
        let unitref = $(this).attr("data-uref");
        let hasRecipe = $(this).attr("data-hasrecipe") === "true";

        if (targetSelection === "main") {
            $("#txtMainProductCode").val(code + " - " + name).attr("data-purecode", code);
            $("#hdnMainProductRef").val(ref);
            $("#txtMainUnit").val(unit);
            $("#hdnMainUnitRef").val(unitref);
            $("#numMainQuantity").val("1.000000");

            $("#btnAddSubProduct").prop("disabled", false);
            $("#btnSaveAllChanges").prop("disabled", false);

            if (hasRecipe) {
                $("#trEmptyRow td").html('<div class="spinner-border spinner-border-sm text-primary me-2"></div> Mevcut reçete satırları Logo\'dan getiriliyor...');

                $.ajax({
                    url: '/Bom/RecipeManagement/GetRecipeLines',
                    type: 'GET',
                    // ARTIK HEM REF HEM DE KODU TAM GÖNDERİYORUZ!
                    data: { mainProductRef: parseInt(ref), mainProductCode: code },
                    success: function (res) {
                        if (res.isSuccess && res.data && res.data.length > 0) {
                            $("#tblRecipeLines tbody").find("tr:not(#trEmptyRow)").remove();
                            $("#trEmptyRow").hide();
                            currentLineCount = 0;

                            res.data.forEach(function (line) {
                                currentLineCount++;
                                let uniqueId = "row_" + currentLineCount;

                                let pCode = line.subProductCode || '';
                                let pName = line.subProductName || '';
                                let pQty = parseFloat(line.subQuantity || 0).toFixed(6);
                                let pUnit = line.subUnit || '';
                                let pRef = line.altUrunRef || 0;
                                let pUnitRef = line.altBirimRef || 1;

                                let row = `<tr id="${uniqueId}" data-db-status="original" data-subref="${pRef}" data-subunitref="${pUnitRef}">
                                    <td class="font-monospace fw-bold text-center row-number">${currentLineCount}</td>
                                    <td class="fw-bold text-secondary">${pCode}</td>
                                    <td>${pName}</td>
                                    <td>
                                        <input type="number" class="form-control form-control-sm txt-sub-qty" value="${pQty}" step="any" style="max-width:120px;">
                                    </td>
                                    <td><span class="badge bg-light text-dark border">${pUnit}</span></td>
                                    <td class="text-center">
                                        <button class="btn btn-sm btn-outline-danger btn-delete-line" data-line="${currentLineCount}">
                                            <i class="bi bi-trash"></i> Kaldır
                                        </button>
                                    </td>
                                </tr>`;
                                $("#tblRecipeLines tbody").append(row);
                            });
                        } else {
                            $("#trEmptyRow").show().find("td").html('<i class="bi bi-info-circle me-1"></i> Reçete satırları tam çözülemedi. Sağ üstten kendiniz hammadde ekleyebilirsiniz.');
                        }
                    },
                    error: function () {
                        $("#trEmptyRow").show().find("td").html('<i class="bi bi-info-circle me-1"></i> Bağlantı hatası. Manuel eklemeye başlayabilirsiniz.');
                    }
                });
            } else {
                $("#tblRecipeLines tbody").find("tr:not(#trEmptyRow)").remove();
                $("#trEmptyRow").show().find("td").html('<i class="bi bi-info-circle me-1"></i> Bu ürünün henüz bir reçetesi yok. Sağ üstten <b>Bileşen Ekle</b> diyerek yeni reçete oluşturabilirsiniz.');
            }
        }
        else if (targetSelection === "sub") {
            let isAlreadyExists = false;
            $("#tblRecipeLines tbody tr").not("#trEmptyRow").each(function () {
                let existingCode = $(this).find("td:eq(1)").text().trim();
                let dbStatus = $(this).attr("data-db-status");
                if (existingCode === code && dbStatus !== "deleted") {
                    isAlreadyExists = true;
                    return false;
                }
            });

            if (isAlreadyExists) {
                alert("Bu bileşen (`" + code + "`) tabloda zaten mevcut!");
                return;
            }

            $("#trEmptyRow").hide();
            currentLineCount++;

            let uniqueId = "row_" + currentLineCount;
            let newRow = `<tr id="${uniqueId}" data-db-status="new" data-subref="${ref}" data-subunitref="${unitref}">
                <td class="font-monospace fw-bold text-center row-number">${currentLineCount}</td>
                <td class="fw-bold text-secondary">${code}</td>
                <td>${name}</td>
                <td>
                    <input type="number" class="form-control form-control-sm txt-sub-qty" value="1.000000" step="any" style="max-width:120px;">
                </td>
                <td><span class="badge bg-light text-dark border">${unit}</span></td>
                <td class="text-center">
                    <button class="btn btn-sm btn-outline-danger btn-delete-line" data-line="${currentLineCount}">
                        <i class="bi bi-trash"></i> Kaldır
                    </button>
                </td>
            </tr>`;

            $("#tblRecipeLines tbody").append(newRow);
        }

        var modalElement = document.getElementById('productSelectModal');
        var modalInstance = bootstrap.Modal.getInstance(modalElement);
        if (modalInstance) { modalInstance.hide(); }
    });

    // 4. SATIR KALDIRMA MANTIĞI
    $(document).on("click", ".btn-delete-line", function () {
        let $tr = $(this).closest("tr");
        let dbStatus = $tr.attr("data-db-status");

        if (dbStatus === "new") {
            $tr.remove();
        } else {
            $tr.attr("data-db-status", "deleted").hide();
        }

        reIndexTableRows();
        checkTableEmptyState();
    });

    function reIndexTableRows() {
        let index = 1;
        $("#tblRecipeLines tbody tr:visible").not("#trEmptyRow").each(function () {
            $(this).find(".row-number").text(index);
            $(this).find(".btn-delete-line").attr("data-line", index);
            index++;
        });
        currentLineCount = index - 1;
    }

    function checkTableEmptyState() {
        let visibleRows = $("#tblRecipeLines tbody tr:visible").not("#trEmptyRow").length;
        if (visibleRows === 0) {
            $("#trEmptyRow").show().find("td").html('<i class="bi bi-info-circle me-1"></i> Reçete satırı kalmadı. Sağ üstten yeni bileşen ekleyebilirsiniz.');
        } else {
            $("#trEmptyRow").hide();
        }
    }

    $("#btnClearForm").click(function () {
        if (confirm("Tüm formu sıfırlamak istediğinize emin misiniz?")) {
            location.reload();
        }
    });

    // 5. TOPLU KAYDETME (BULK SAVE)
    $("#btnSaveAllChanges").on("click", function (e) {
        e.preventDefault();
        let $btn = $(this);
        let mainProductRef = parseInt($("#hdnMainProductRef").val()) || 0;
        let mainQuantity = parseFloat($("#numMainQuantity").val()) || 1.0;
        let mainUnitRef = parseInt($("#hdnMainUnitRef").val()) || 1;

        let payload = {
            AnaUrunRef: mainProductRef,
            AnaMiktar: mainQuantity,
            AnaBirimRef: mainUnitRef,
            IsDeleteAll: false,
            Lines: []
        };

        $("#tblRecipeLines tbody tr").not("#trEmptyRow").each(function () {
            let $tr = $(this);
            payload.Lines.push({
                Status: $tr.attr("data-db-status"),
                SatirNo: parseInt($tr.find(".row-number").text()) || 0,
                AltUrunRef: parseInt($tr.attr("data-subref")),
                AltBirimRef: parseInt($tr.attr("data-subunitref")),
                AltMiktar: parseFloat($tr.find(".txt-sub-qty").val()) || 0
            });
        });

        let activeCount = payload.Lines.filter(l => l.Status !== "deleted").length;
        if (activeCount === 0 && payload.Lines.length > 0) {
            if (confirm("Tüm satırları sildiniz. REÇETEYİ LOGO'DAN KOMPLE SİLMEK İSTİYOR MUSUNUZ?")) {
                payload.IsDeleteAll = true;
            } else {
                return;
            }
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
                    location.reload();
                } else {
                    alert("Hata: " + res.message);
                    $btn.prop("disabled", false).html('<i class="bi bi-cloud-check-fill me-2"></i> Tüm Değişiklikleri Kaydet');
                }
            }
        });
    });
});