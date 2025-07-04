window.renderChart = function (data) {
    const ctx = document.getElementById('npvChart').getContext('2d');

    if (!ctx) return;

    if (window.myChart) {
        window.myChart.destroy();
    }

    window.myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.discountRate),
            datasets: [{
                label: 'NPV',
                data: data.map(d => d.npv),
                borderWidth: 2,
                fill: true,
                tension: 0.2
            }]
        },
        options: {
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Discount Rate (%)'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'NPV Value'
                    },
                    beginAtZero: false
                }
            }
        }
    });
};
