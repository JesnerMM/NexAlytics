// NEXAlytics — Import drag & drop
(function () {
    'use strict';

    const dropzone = document.getElementById('dropzone');
    const fileInput = document.getElementById('fileInput');
    const uploadBtn = document.getElementById('uploadBtn');
    const fileSelected = document.getElementById('fileSelected');
    const fileNameSpan = document.getElementById('fileName');

    if (!dropzone) return;

    // Format helper panels
    const radios = document.querySelectorAll('input[name="tipo"]');
    radios.forEach(function (radio) {
        radio.addEventListener('change', function () {
            document.querySelectorAll('[id^="format"]').forEach(el => el.classList.add('d-none'));
            const panel = document.getElementById('format' + radio.value);
            if (panel) panel.classList.remove('d-none');
        });
    });

    function handleFile(file) {
        if (!file) return;
        const allowed = ['.csv', '.xlsx', '.xls'];
        const ext = '.' + file.name.split('.').pop().toLowerCase();
        if (!allowed.includes(ext)) {
            alert('Formato no soportado. Use CSV o Excel.');
            return;
        }
        fileNameSpan.textContent = file.name + ' (' + (file.size / 1024).toFixed(1) + ' KB)';
        fileSelected.classList.remove('d-none');
        uploadBtn.removeAttribute('disabled');
    }

    fileInput.addEventListener('change', function () {
        handleFile(this.files[0]);
    });

    dropzone.addEventListener('dragover', function (e) {
        e.preventDefault();
        dropzone.classList.add('drag-over');
    });

    dropzone.addEventListener('dragleave', function () {
        dropzone.classList.remove('drag-over');
    });

    dropzone.addEventListener('drop', function (e) {
        e.preventDefault();
        dropzone.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) {
            // Create a DataTransfer to set fileInput.files
            const dt = new DataTransfer();
            dt.items.add(file);
            fileInput.files = dt.files;
            handleFile(file);
        }
    });
})();
