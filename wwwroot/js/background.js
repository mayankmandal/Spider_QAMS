// background.js
window.onload = function () {
    const canvas = document.getElementById('three-background');
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    const renderer = new THREE.WebGLRenderer({ canvas: canvas });

    console.log('Hello Hi');

    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    // Create a line geometry for abstract strings
    const geometry = new THREE.BufferGeometry();
    const positions = [];
    const numLines = 500;

    // Random points for abstract string-like effect
    for (let i = 0; i < numLines; i++) {
        positions.push(Math.random() * 2 - 1);
        positions.push(Math.random() * 2 - 1);
        positions.push(Math.random() * 2 - 1);
    }
    geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));

    const material = new THREE.LineBasicMaterial({ color: 0xffffff, linewidth: 2 });
    const line = new THREE.LineSegments(geometry, material);
    scene.add(line);

    // Mouse movement interaction to move the camera
    let mouseX = 0;
    let mouseY = 0;
    document.addEventListener('mousemove', (event) => {
        mouseX = (event.clientX / window.innerWidth) * 2 - 1;
        mouseY = -(event.clientY / window.innerHeight) * 2 + 1;
    });

    camera.position.z = 5;

    // Animation loop
    function animate() {
        requestAnimationFrame(animate);

        // Apply mouse interaction to camera position
        camera.position.x += (mouseX - camera.position.x) * 0.05;
        camera.position.y += (-mouseY - camera.position.y) * 0.05;
        camera.lookAt(scene.position);

        // Rotate the lines for dynamic effect
        line.rotation.x += 0.001;
        line.rotation.y += 0.001;

        renderer.render(scene, camera);
    }

    animate();

    // Adjust renderer and camera on window resize
    window.addEventListener('resize', () => {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
    });
};