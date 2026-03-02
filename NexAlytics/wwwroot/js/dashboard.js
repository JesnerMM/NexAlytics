// NEXAlytics — Dashboard Charts
(function () {
    'use strict';

    const palette = {
        primary: 'rgba(30, 58, 95, 0.85)',
        primaryBorder: '#1e3a5f',
        accent: 'rgba(0, 198, 255, 0.8)',
        success: 'rgba(16, 185, 129, 0.8)',
        warning: 'rgba(245, 158, 11, 0.8)',
        danger: 'rgba(239, 68, 68, 0.8)',
        info: 'rgba(59, 130, 246, 0.8)',
        colors: [
            '#1e3a5f', '#00c6ff', '#10b981', '#f59e0b',
            '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4'
        ]
    };

    const defaultOptions = {
        responsive: true,
        plugins: {
            legend: { position: 'bottom', labels: { font: { size: 12 }, padding: 16 } }
        }
    };

    // Bar chart: Ventas por mes
    const ctxBar = document.getElementById('chartVentasMes');
    if (ctxBar && typeof ventasMesData !== 'undefined') {
        new Chart(ctxBar, {
            type: 'bar',
            data: {
                labels: ventasMesData.map(d => d.label),
                datasets: [{
                    label: 'Ventas ($)',
                    data: ventasMesData.map(d => d.value),
                    backgroundColor: palette.primary,
                    borderColor: palette.primaryBorder,
                    borderWidth: 1,
                    borderRadius: 6
                }]
            },
            options: {
                ...defaultOptions,
                plugins: { ...defaultOptions.plugins, legend: { display: false } },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { callback: v => '$' + v.toLocaleString() }
                    }
                }
            }
        });
    }

    // Doughnut chart: Ventas por estado
    const ctxDona = document.getElementById('chartVentasEstado');
    if (ctxDona && typeof ventasEstadoData !== 'undefined') {
        new Chart(ctxDona, {
            type: 'doughnut',
            data: {
                labels: ventasEstadoData.map(d => d.label),
                datasets: [{
                    data: ventasEstadoData.map(d => d.value),
                    backgroundColor: palette.colors,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                ...defaultOptions,
                cutout: '65%'
            }
        });
    }
})();
