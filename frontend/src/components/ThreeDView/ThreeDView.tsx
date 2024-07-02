import React, { useRef, useEffect } from "react";
import * as THREE from "three";
import {
  CSS3DObject,
  CSS3DRenderer,
  OrbitControls,
} from "three/examples/jsm/Addons.js";
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
    camera.position.set(0, 5, 10);

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(mount.clientWidth, mount.clientHeight);
    mount.appendChild(renderer.domElement);

    const cssRenderer = new CSS3DRenderer();
    cssRenderer.setSize(mount.clientWidth, mount.clientHeight);
    cssRenderer.domElement.style.position = "absolute";
    cssRenderer.domElement.style.top = "0";
    cssRenderer.domElement.style.pointerEvents = "none"; // Allow mouse events to pass through
    mount.appendChild(cssRenderer.domElement);

    // Initialize Leaflet map
    mapElement.style.display = "block"; // Ensure the element is visible
    mapElement.style.pointerEvents = "none"; // Ensure the element does not consume events
    const map = L.map(mapElement).setView([51.505, -0.09], 13);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: "© OpenStreetMap contributors",
    }).addTo(map);

    const cssObject = new CSS3DObject(mapElement);
    cssObject.rotation.x = -Math.PI / 2; // Rotate to lie flat
    cssObject.position.set(0, 0, 0); // Position at ground level
    scene.add(cssObject);

    // Light
    const light = new THREE.DirectionalLight(0xffffff, 1);
    light.position.set(10, 10, 10).normalize();
    scene.add(light);

    // Controls
    const controls = new OrbitControls(camera, renderer.domElement);

    // Keyboard controls
    const moveSpeed = 0.5;
    const keyState: { [key: string]: boolean } = {};

    const onKeyDown = (event: KeyboardEvent) => {
      keyState[event.code] = true;
    };

    const onKeyUp = (event: KeyboardEvent) => {
      keyState[event.code] = false;
    };

    const updateCameraPosition = () => {
      if (keyState["ArrowUp"] || keyState["KeyW"]) {
        camera.position.z -= moveSpeed;
      }
      if (keyState["ArrowDown"] || keyState["KeyS"]) {
        camera.position.z += moveSpeed;
      }
      if (keyState["ArrowLeft"] || keyState["KeyA"]) {
        camera.position.x -= moveSpeed;
      }
      if (keyState["ArrowRight"] || keyState["KeyD"]) {
        camera.position.x += moveSpeed;
      }
    };

    window.addEventListener("keydown", onKeyDown);
    window.addEventListener("keyup", onKeyUp);

    // Animate
    const animate = () => {
      requestAnimationFrame(animate);
      updateCameraPosition();
      controls.update();
      renderer.render(scene, camera);
      cssRenderer.render(scene, camera);
    };
    animate();

    // Cleanup on component unmount
    return () => {
      mount.removeChild(renderer.domElement);
      mount.removeChild(cssRenderer.domElement);
      map.remove();
      window.removeEventListener("keydown", onKeyDown);
      window.removeEventListener("keyup", onKeyUp);
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
          width: "1600px",
          height: "1600px",
          position: "absolute",
          display: "none", // Initially hidden
        }}
      />
    </>
  );
};

export default ThreeDView;
