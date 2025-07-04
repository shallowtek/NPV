window.renderChart = (data) => {
    var ctx = document.getElementById('npvChart').getContext('2d');
    var chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.discountRate),
            datasets: [{
                label: 'NPV vs Discount Rate',
                data: data.map(d => d.npv),
                borderColor: 'rgba(75, 192, 192, 1)',
                fill: false,
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
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
                        text: 'NPV'
                    }
                }
            }
        });
};
