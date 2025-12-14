// Replace bare imports with ESM CDN URLs so the browser can resolve them
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.22.1/firebase-app.js";
import { getAnalytics } from "https://www.gstatic.com/firebasejs/9.22.1/firebase-analytics.js";
import { getDatabase, ref, child, get } from "https://www.gstatic.com/firebasejs/9.22.1/firebase-database.js";

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyBDbLUe_lzZIzZVb35mouoLmYcERJEHWrI",
  authDomain: "itdxdda-asg1-48367.firebaseapp.com",
  databaseURL: "https://itdxdda-asg1-48367-default-rtdb.asia-southeast1.firebasedatabase.app",
  projectId: "itdxdda-asg1-48367",
  storageBucket: "itdxdda-asg1-48367.firebasestorage.app",
  messagingSenderId: "713129832960",
  appId: "1:713129832960:web:935e0d6cc1d43f6c371ee5",
  measurementId: "G-HQ0P32PZNW"
};

// module-scoped handles
let app = null;
let db = null;

export function initDB() {
  if (!app) {
    app = initializeApp(firebaseConfig);
    try { getAnalytics(app); } catch (e) { /* analytics may fail in some environments */ }
    db = getDatabase(app);
  }
  return db;
}

/**
 * fetchItems(path)
 * path: RTDB path string like 'FoodClubChickenRice/FoodStats'
 * returns: parsed object or null
 */
export async function fetchItems(path = '/') {
  if (!db) initDB();
  const dbRef = ref(db);
  const snapshot = await get(child(dbRef, path));
  return snapshot.exists() ? snapshot.val() : null;
}

export async function initAndPopulate() {
  const humanize = k =>
    (k || '')
      .replace(/[_\-]+/g, ' ')
      .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
      .replace(/\s+/g, ' ')
      .trim() || 'Untitled';

  try {
    initDB();

    // first card: FoodClubChickenRice -> SteamedChickenRice
    const steamedChickenNode = await fetchItems('FoodClubChickenRice/FoodStats/SteamedChickenRice');
    if (steamedChickenNode) {
      const el = document.getElementById('dish-name');
      const descEl = document.getElementById('dish-desc');
      if (el) {
        const label = steamedChickenNode.name || steamedChickenNode.title || humanize('SteamedChickenRice');
        el.textContent = label;
      }
      if (descEl && steamedChickenNode.Description) {
        descEl.textContent = steamedChickenNode.Description;
      }
    }

    // second card: fetch the Bakednode directly and set its display name
    const BakedChickenRiceNode = await fetchItems('FoodClubTurkish/FoodStats/BakedChickenRice');
    if (BakedChickenRiceNode) {
      const el2 = document.getElementById('dish2-name');
      const desc2 = document.getElementById('dish2-desc');
      if (el2) {
        // prefer explicit fields, fall back to humanized key
        const label = BakedChickenRiceNode.name || BakedChickenRiceNode.title || humanize('BakedChickenRice');
        el2.textContent = label;
      }
      if (desc2 && BakedChickenRiceNode.Description) {
        desc2.textContent = BakedChickenRiceNode.Description;
      }
    }

    const chickenKebabNode = await fetchItems('FoodClubTurkish/FoodStats/ChickenKebab');
    if (chickenKebabNode) {
      const el2 = document.getElementById('dish3-name');
      const desc3 = document.getElementById('dish3-desc');
      if (el2) {
        // prefer explicit fields, fall back to humanized key
        const label = chickenKebabNode.name || chickenKebabNode.title || humanize('ChickenKebab');
        el2.textContent = label;
      }
      if (desc3 && chickenKebabNode.Description) {
        desc3.textContent = chickenKebabNode.Description;
      }
    }

    const chickencutletriceNode = await fetchItems('FoodClubChickenRice/FoodStats/ChickenCutletRice');
    if (chickencutletriceNode) {
      const el2 = document.getElementById('dish4-name');
      const desc4 = document.getElementById('dish4-desc');
      if (el2) {
        // prefer explicit fields, fall back to humanized key
        const label = chickencutletriceNode.name || chickencutletriceNode.title || humanize('ChickenCutletRice');
        el2.textContent = label;
      }
      if (desc4 && chickencutletriceNode.Description) {
        desc4.textContent = chickencutletriceNode.Description;
      }
    }

  } catch (err) {
    console.error('initAndPopulate error:', err);
  }
}