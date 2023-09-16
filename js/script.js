const container = document.getElementById("container");
let clock = new THREE.Clock();
const gui = new dat.GUI();
gui.hide();

//custom events
let sceneLoaded = false;
const sceneLoadedEvent = new Event("sceneLoaded");
const sceneChanged = new Event("sceneChanged");

let isDebug = false;
let isPaused = false;
let currentScene = null;
let scene, camera, renderer, material;
let settings = { fps: 24, scale: 1, parallaxVal: 0 };
//required: u_mouse, u_time, u_brightness, u_resolution, u_tex0_resolution,
let shaders = [
  {
    name: "rain",
    uniform: {
      u_tex0: { type: "t" },
      u_time: { value: 0, type: "f" },
      u_blur: { value: false, type: "b" },
      u_intensity: { value: 0.3, type: "f" },
      u_speed: { value: 0.25, type: "f" },
      u_brightness: { value: 0.75, type: "f" },
      u_normal: { value: 0.5, type: "f" },
      u_zoom: { value: 2.61, type: "f" },
      u_panning: { value: false, type: "b" },
      u_post_processing: { value: true, type: "b" },
      u_lightning: { value: false, type: "b" },
      u_mouse: { value: new THREE.Vector4(), type: "v4" },
      u_resolution: { value: new THREE.Vector2(window.innerWidth, window.innerHeight), type: "v2" },
      u_tex0_resolution: { value: new THREE.Vector2(window.innerWidth, window.innerHeight), type: "v2" },
    },
    fragmentShaderPath: "shaders/rain.frag",
    scale: 0.75,
  },
];
const quad = new THREE.Mesh(new THREE.PlaneGeometry(2, 2, 1, 1));
let videoElement;

let vertexShader = `
varying vec2 vUv;        
void main() {
    vUv = uv;
    gl_Position = vec4( position, 1.0 );    
}
`;

async function init() {
  renderer = new THREE.WebGLRenderer({
    antialias: false,
    preserveDrawingBuffer: false,
  });
  renderer.setSize(window.innerWidth, window.innerHeight);
  renderer.setPixelRatio(settings.scale);
  container.appendChild(renderer.domElement);
  scene = new THREE.Scene();
  camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
  scene.add(quad);

 //caching for textureloader
  //ref: https://threejs.org/docs/#api/en/loaders/Cache
  THREE.Cache.enabled = true;

  await setScene("rain");
  render(); //since init is async

  window.addEventListener("resize", (e) => resize());

  if (isDebug) {
    debugMenu();
    gui.show();
  } else {
    gui.hide();
  }
}

//Pause rendering
function setPause(val) {
  isPaused = val;
}

function setScale(value) {
  if (settings.scale == value) return;

  settings.scale = value;
  renderer.setPixelRatio(settings.scale);
  material.uniforms.u_resolution.value = new THREE.Vector2(
    window.innerWidth * settings.scale,
    window.innerHeight * settings.scale
  );
}

async function setScene(name, geometry = quad) {
  if (name == currentScene) return;
  currentScene = name;

  material?.uniforms?.u_tex0?.value?.dispose();
  material?.dispose();
  disposeVideoElement(videoElement);

  switch (name) {
    case "rain":
      {
        material = new THREE.ShaderMaterial({
          uniforms: shaders[0].uniform,
          vertexShader: vertexShader,
          fragmentShader: await (await fetch(shaders[0].fragmentShaderPath)).text(),
        });
        setScale(shaders[0].scale);
        material.uniforms.u_tex0_resolution.value = new THREE.Vector2(1920, 1080);
        material.uniforms.u_tex0.value = await new THREE.TextureLoader().loadAsync("media/mountain.webp");
      }
      break;
  }
  geometry.material = material;
  resize(); //update view

  if (!sceneLoaded) {
    sceneLoaded = true;
    document.dispatchEvent(sceneLoadedEvent);
  }
  document.dispatchEvent(sceneChanged);
}

function resize() {
  renderer.setSize(window.innerWidth, window.innerHeight);
  material.uniforms.u_resolution.value = new THREE.Vector2(
    window.innerWidth * settings.scale,
    window.innerHeight * settings.scale
  );
}

function render() {
  setTimeout(function () {
    requestAnimationFrame(render);
  }, 1000 / settings.fps);

  //reset every 6hr
  if (clock.getElapsedTime() > 21600) clock = new THREE.Clock();
  material.uniforms.u_time.value = clock.getElapsedTime();

  if (!isPaused) renderer.render(scene, camera);
}

init();

//helpers
//ref: https://stackoverflow.com/questions/3258587/how-to-properly-unload-destroy-a-video-element
function disposeVideoElement(video) {
  if (video != null && video.hasAttribute("src")) {
    video.pause();
    video.removeAttribute("src"); // empty source
    video.load();
  }
}

//debug
function debugMenu() {
  try {
    debugScale();
    debugCloud();
  } catch (ex) {
    console.log(ex);
  }
}

function get_random (list) {
  return list[Math.floor((Math.random()*list.length))];
}

function debugScale() {
  gui
    .add(settings, "scale", 0.1, 2, 0.01)
    .name("Display Scale")
    .onChange(function () {
      setScale(settings.scale);
    });
}

function debugCloud() {
  gui.add(material.uniforms.u_fog, "value").name("Fog");
  gui.add(material.uniforms.u_scale, "value", 0, 2, 0.01).name("Size1");
}
