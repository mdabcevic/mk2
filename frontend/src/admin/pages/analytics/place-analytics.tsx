import React from 'react';
import { MapContainer, TileLayer, CircleMarker, Tooltip } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import { PlaceTraffic } from "./analytics-interface";
import { LatLngTuple } from 'leaflet';
import { useTranslation } from "react-i18next";

interface PlacesTraffic {
  data: PlaceTraffic[];
}

const PlaceMap: React.FC<PlacesTraffic> = ({ data }) => {  
  if (!data || data.length === 0) {
    return <p>Nema dostupnih podataka za prikaz mape.</p>;
  }

  const calculateCenter = (): LatLngTuple => {
    
    const lats = data.map(cafe => cafe.lat);
    const longs = data.map(cafe => cafe.long);

    const minLat = Math.min(...lats);
    const maxLat = Math.max(...lats);
    const minLong = Math.min(...longs);
    const maxLong = Math.max(...longs);
    
    return [
      (minLat + maxLat) / 2,
      (minLong + maxLong) / 2
    ];
  };

  const calculateZoom = () => {
    const lats = data.map(cafe => cafe.lat);
    const longs = data.map(cafe => cafe.long);
    
    const latRange = Math.max(...lats) - Math.min(...lats);
    const longRange = Math.max(...longs) - Math.min(...longs);
    const maxRange = Math.max(latRange, longRange);
    
    if (maxRange < 0.01) return 15;
    if (maxRange < 0.05) return 13;
    if (maxRange < 0.1) return 11;
    if (maxRange < 0.5) return 9;
    return 7;
  };

  const center = calculateCenter() as LatLngTuple;
  const zoom = calculateZoom();
  const maxTraffic = Math.max(...data.map(cafe => cafe.count));
  const { t } = useTranslation("admin");

  return (
    <MapContainer 
      center={ center }
      zoom={ zoom } 
      style={{ height: '400px', width: '100%' }}
    >
      <TileLayer
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        //url="https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png"
        //attribution='&copy; <a href="https://carto.com/">CARTO</a>'
      />
      
      {data.map(cafe => {
        // Izračunaj boju i veličinu na temelju prometa
        const intensity = cafe.count / maxTraffic;
        const radius = 5 + intensity * 20; // Osnovni radius + dodatak ovisno o prometu
        const hue = 120 - (intensity * 120); // Zelena do crvena
        const color = `hsl(${hue}, 100%, 50%)`;
        
        return (
          <CircleMarker
            key={cafe.placeId}
            center={[cafe.lat, cafe.long]}
            radius={radius}
            fillOpacity={0.7}
            color={color}
            fillColor={color}
          >
            <Tooltip direction="top" opacity={0.9}>
              {cafe.address} - {t("analytics.traffic")} : {cafe.count}
            </Tooltip>
          </CircleMarker>
        );
      })}
    </MapContainer>
  );
};

export default PlaceMap;