import { useEffect, useState } from 'react';
import './App.css';

interface Forecast {
    country: string;
    city: string;
    temp: number;
    tempMin: number;
    tempMax: number;
    timestamp: string;
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
                <div>
                    <h2>{p.country} - {p.city}</h2>
                    <table className="table table-striped" aria-labelledby="tableLabel">
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Temperature</th>
                                <th>Min Temperature</th>
                                <th>Max Temperature</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filter(p, forecasts).map(forecast =>
                                <tr key={forecast.timestamp}>
                                    <td>{forecast.timestamp}</td>
                                    <td>{forecast.temp}</td>
                                    <td>{forecast.tempMin}</td>
                                    <td>{forecast.tempMax}</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>)}
            </div>;
        }
    }

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        const data = await response.json();
        setForecasts(data);
    }
}

export default App;