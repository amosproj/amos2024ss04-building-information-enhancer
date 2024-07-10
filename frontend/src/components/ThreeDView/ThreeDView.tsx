import React, { useRef, useEffect } from "react";
import * as THREE from "three";
import {
  CSS3DObject,
  CSS3DRenderer,
} from "three/examples/jsm/renderers/CSS3DRenderer.js";
import { MapControls } from "three/examples/jsm/controls/MapControls.js";
import "leaflet/dist/leaflet.css";
import LeafletMap from "../MapView/LeafletMap";

interface ThreeDViewProps {
  datasetId: string;
  mapType: string;
}

const ThreeDView: React.FC<ThreeDViewProps> = ({ datasetId, mapType }) => {
  const mountRef = useRef<HTMLDivElement>(null);
  const threedMapRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const mount = mountRef.current!;
    const mapElement = threedMapRef.current!;

    // Scene setup
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x87ceeb); // Light blue sky
    const camera = new THREE.PerspectiveCamera(
      75,
      mount.clientWidth / mount.clientHeight,
      0.1,
      1000
    );
    camera.position.set(0, 400, 10);
    camera.rotateX(-1);

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

    const cssObject = new CSS3DObject(mapElement);
    cssObject.rotation.x = -Math.PI / 2; // Rotate to lie flat
    cssObject.position.set(0, 0, 0); // Position at ground level
    scene.add(cssObject);

    // Light
    const light = new THREE.DirectionalLight(0xffffff, 1);
    light.position.set(10, 10, 10).normalize();
    scene.add(light);

    // Set up MapControls
    const controls = new MapControls(camera, cssRenderer.domElement);
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
    const forwardEventToMap = (event: Event) => {
      if (event instanceof WheelEvent) {
        if (event.deltaY < 0) {
          // Simulate zoom in
          const simulatedEvent = new WheelEvent(event.type, {
            ...event,
            deltaY: -1,
          });
          mapElement.dispatchEvent(simulatedEvent);
        } else {
          // Simulate zoom out
          const simulatedEvent = new WheelEvent(event.type, {
            ...event,
            deltaY: 1,
          });
          mapElement.dispatchEvent(simulatedEvent);
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
    const events: (keyof HTMLElementEventMap)[] = ["mousedown", "wheel"];
    events.forEach((eventName) => {
      renderer.domElement.addEventListener(
        eventName,
        forwardEventToMap as EventListener
      );
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
    };
  }, []);

  return (
    <div ref={mountRef} className="tab-map-container">
      <div
        ref={threedMapRef}
        style={{
          width: "500px",
          height: "500px",
          position: "absolute",
          zIndex: -1,
        }}
      >
        <LeafletMap datasetId={datasetId} mapType={mapType} />
      </div>
    </div>
  );
};

export default ThreeDView;
