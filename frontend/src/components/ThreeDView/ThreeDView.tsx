import React, { useRef, useEffect } from "react";
import * as THREE from "three";
import { OrbitControls } from "three/examples/jsm/Addons.js";

const ThreeDView: React.FC = () => {
  const mountRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const mount = mountRef.current!;

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

    // Plane
    const planeGeometry = new THREE.PlaneGeometry(100, 100);
    const planeMaterial = new THREE.MeshBasicMaterial({ color: 0x808080 });
    const plane = new THREE.Mesh(planeGeometry, planeMaterial);
    plane.rotation.x = -Math.PI / 2;
    scene.add(plane);

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
    };
    animate();

    // Cleanup on component unmount
    return () => {
      mount.removeChild(renderer.domElement);
      window.removeEventListener("keydown", onKeyDown);
      window.removeEventListener("keyup", onKeyUp);
    };
  }, []);

  return <div ref={mountRef} style={{ width: "100%", height: "100vh" }} />;
};

export default ThreeDView;
