import React, { useRef, useEffect } from "react";
import * as THREE from "three";
import {
  CSS3DObject,
  CSS3DRenderer,
} from "three/examples/jsm/renderers/CSS3DRenderer.js";
import { MapControls } from "three/examples/jsm/controls/MapControls.js";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

const ThreeDView: React.FC = () => {
  const mountRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const mount = mountRef.current!;
    const mapElement = mapRef.current!;

    // Scene setup
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x87ceeb); // Light blue sky
    const camera = new THREE.PerspectiveCamera(
      75,
      mount.clientWidth / mount.clientHeight,
      0.1,
      1000
    );
    camera.position.set(0, 300, 10);
    camera.rotateX(-0.7);

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(mount.clientWidth, mount.clientHeight);
    mount.appendChild(renderer.domElement);

    const cssRenderer = new CSS3DRenderer();
    cssRenderer.setSize(mount.clientWidth, mount.clientHeight);
    cssRenderer.domElement.style.position = "absolute";
    cssRenderer.domElement.style.top = "0";
    cssRenderer.domElement.style.pointerEvents = "none"; // Allow mouse events to pass through
    mount.appendChild(cssRenderer.domElement);

    // Ensure the map container is displayed and has dimensions before initializing Leaflet
    mapElement.style.display = "block"; // Ensure the element is visible
    mapElement.style.pointerEvents = "auto"; // Ensure the element can consume events

    // Initialize Leaflet map
    const map = L.map(mapElement).setView([51.505, -0.09], 13);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: "© OpenStreetMap contributors",
    }).addTo(map);

    const cssObject = new CSS3DObject(mapElement);
    cssObject.rotation.x = -Math.PI / 2; // Rotate to lie flat
    cssObject.position.set(0, 0, 0); // Position at ground level
    scene.add(cssObject);

    // Force Leaflet map to re-render
    setTimeout(() => {
      map.invalidateSize();
    }, 0);

    // Light
    const light = new THREE.DirectionalLight(0xffffff, 1);
    light.position.set(10, 10, 10).normalize();
    scene.add(light);

    // Set up MapControls
    const controls = new MapControls(camera, renderer.domElement);
    controls.enableDamping = true; // Enable damping (inertia)
    controls.dampingFactor = 0.05; // Damping factor
    controls.screenSpacePanning = false; // Prevent camera from moving vertically in screen space
    controls.minDistance = 10; // Minimum zoom distance
    controls.maxDistance = 500; // Maximum zoom distance
    controls.maxPolarAngle = Math.PI / 2; // Limit angle from the top

    // Configure controls to respond only to right-click
    controls.mouseButtons = {
      LEFT: null,
      MIDDLE: null,
      RIGHT: THREE.MOUSE.ROTATE,
    };
    controls.enableZoom = false;

    // Function to forward events to Leaflet map
    const forwardEventToMap = (event: MouseEvent | WheelEvent) => {
      if (event instanceof WheelEvent) {
        if (event.deltaY < 0) {
          map.zoomIn();
        } else {
          map.zoomOut();
        }
      } else if (event instanceof MouseEvent && event.button !== 2) {
        // Prevent right-click events from being forwarded
        const simulatedEvent = new MouseEvent(event.type, {
          bubbles: true,
          cancelable: true,
          clientX: event.clientX - 1,
          clientY: event.clientY - 1,
        });
        mapElement.dispatchEvent(simulatedEvent);
      }
    };
    // Add event listeners to forward events
    const events = ["mousedown", "wheel"];
    events.forEach((eventName) => {
      renderer.domElement.addEventListener(eventName, forwardEventToMap);
    });

    // Animate
    const animate = () => {
      requestAnimationFrame(animate);
      controls.update(); // Update controls
      renderer.render(scene, camera);
      cssRenderer.render(scene, camera);
    };
    animate();

    // Cleanup on component unmount
    return () => {
      mount.removeChild(renderer.domElement);
      mount.removeChild(cssRenderer.domElement);
      map.remove();
    };
  }, []);

  return (
    <>
      <div
        ref={mountRef}
        style={{ width: "100%", height: "100vh", position: "relative" }}
      />
      <div
        ref={mapRef}
        style={{
          width: "2000px",
          height: "2000px",
          position: "absolute",
          zIndex: -999,
        }}
      />
    </>
  );
};

export default ThreeDView;
