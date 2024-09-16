import { useEffect, useState } from 'react';
import { Line } from "react-chartjs-2";
import { Chart as ChartJS, registerables } from 'chart.js/auto';
import 'chartjs-adapter-moment';
import './App.css';

ChartJS.register(...registerables);

interface Forecast {
    country: string;
    city: string;
    temp: number;
    tempMin: number;
    tempMax: number;
    timestamp: Date;
}

interface Location {
    country: string;
    city: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();

    useEffect(() => {
        populateWeatherData();

        const timer = setInterval(() => {
            populateWeatherData();
        }, 60000);

        return () => {
            clearInterval(timer);
        };
    }, []);

    return (
        <div>
            <h1 id="tableLabel">Weather forecast</h1>
            {getContent()}
        </div>
    );

    function getContent() {
        if (forecasts === undefined) {
            return <p><em>Loading... </em></p>;
        } else {
            const filter = (place: Location, forecasts: Forecast[]) => forecasts.filter(f => f.country === place.country && f.city === place.city);

            const places: Location[] = [];
            forecasts.map(f => {
                if (places.find(p => f.city === p.city && f.country === p.country) === undefined)
                    places.push({ country: f.country, city: f.city } as Location);
            });

            return <div>{places.map(p =>
                <div style={{ width: "70vw", height: "30vw" }} >
                    {createGraph(p, filter(p, forecasts))}
                </div>)}
            </div>;
        }
    }

    function createGraph(location: Location, forecasts: Forecast[]) {
        const dates: number[] = forecasts.map(f => (new Date(f.timestamp)).getTime());
        const lastUpdate: Date = new Date(Math.max(...dates));

        return <Line data={{
            labels: forecasts.map(forecast => forecast.timestamp),
            datasets: [{
                label: "Temperature",
                data: forecasts.map(forecast => forecast.temp),
                backgroundColor: "#E05C5C",
                borderColor: "#E05C5C"
            },
            {
                label: "Min",
                data: forecasts.map(forecast => forecast.tempMin),
                fill: false,
                borderDash: [5, 5],
                backgroundColor: "#36A2EB",
                borderColor: "#36A2EB"
            },
            {
                label: "Max",
                data: forecasts.map(forecast => forecast.tempMax),
                fill: false,
                borderDash: [5, 5],
                backgroundColor: "#4BC0C0",
                borderColor: "#4BC0C0"
            }],
        }}
            options={{
                responsive: true,
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'second',
                            tooltipFormat: 'mm:ss',
                        },
                        ticks: {
                            major: {
                                enabled: true,
                            },
                        },
                    },
                    y: {
                        type: 'linear',
                        suggestedMin: 0,
                        suggestedMax: 20
                    }
                },
                plugins: {
                    title: {
                        display: true,
                        text: location.country + " - " + location.city
                    },
                    subtitle: {
                        display: true,
                        text: "Last Update: " + lastUpdate
                    }
                }
            }}
        />;
    }

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        const data = await response.json();
        setForecasts(data);
    }
}

export default App;