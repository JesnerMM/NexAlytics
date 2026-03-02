// NEXAlytics — Analytics Charts
(function () {
    'use strict';

    const colors = ['#1e3a5f', '#00c6ff', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

    let ventasMesChart = null;
    let pagosMetodoChart = null;
    let categoriaChart = null;

    function buildBarChart(ctx, data, label) {
        if (ventasMesChart) ventasMesChart.destroy();
        ventasMesChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(d => d.label),
                datasets: [{
                    label: label,
                    data: data.map(d => d.value),
                    backgroundColor: 'rgba(30, 58, 95, 0.8)',
                    borderColor: '#1e3a5f',
                    borderWidth: 1,
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, ticks: { callback: v => '$' + v.toLocaleString() } }
                }
            }
        });
    }

    function buildDoughnutChart(ctx, data, existingChart) {
        if (existingChart) existingChart.destroy();
        return new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(d => d.label),
                datasets: [{
                    data: data.map(d => d.value),
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                cutout: '60%',
                plugins: { legend: { position: 'bottom' } }
            }
        });
    }

    window.loadVentasMes = function (months) {
        fetch('/Analytics/VentasPorMes?months=' + months)
            .then(r => r.json())
            .then(data => {
                const ctx = document.getElementById('analyticsVentasMes');
                if (ctx) buildBarChart(ctx, data, 'Ventas ($)');
            })
            .catch(console.error);
    };

    function loadPagosMetodo() {
        fetch('/Analytics/PagosPorMetodo')
            .then(r => r.json())
            .then(data => {
                const ctx = document.getElementById('analyticsPagosMetodo');
                if (ctx) pagosMetodoChart = buildDoughnutChart(ctx, data, pagosMetodoChart);
            })
            .catch(console.error);
    }

    function loadCategoria() {
        fetch('/Analytics/VentasPorCategoria')
            .then(r => r.json())
            .then(data => {
                const ctx = document.getElementById('analyticsCategoria');
                if (ctx) categoriaChart = buildDoughnutChart(ctx, data, categoriaChart);
            })
            .catch(console.error);
    }

    // Initial load
    loadVentasMes(12);
    loadPagosMetodo();
    loadCategoria();

    // Power BI embed
    if (typeof window.pbiConfig !== 'undefined' && window.pbiConfig.embedToken) {
        const container = document.getElementById('embedContainer');
        if (container && window.powerbi) {
            const models = window['powerbi-client'].models;
            window.powerbi.embed(container, {
                type: 'report',
                id: window.pbiConfig.reportId,
                embedUrl: window.pbiConfig.embedUrl,
                accessToken: window.pbiConfig.embedToken,
                tokenType: models.TokenType.Embed,
                settings: {
                    panes: { filters: { visible: false } },
                    background: models.BackgroundType.Transparent
                }
            });
        }
    }
})();
