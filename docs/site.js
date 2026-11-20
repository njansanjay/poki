const clock = document.querySelector('#clock');
function updateClock() {
  const now = new Date();
  clock.dateTime = now.toISOString();
  clock.textContent = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}
updateClock();
setInterval(updateClock, 60000);
const cat = document.querySelector('#cat-button');
const menu = document.querySelector('#shortcut-demo');
function closeMenu() { menu.hidden = true; cat.setAttribute('aria-expanded', 'false'); }
cat.addEventListener('click', () => {
  menu.hidden = !menu.hidden;
  cat.setAttribute('aria-expanded', String(!menu.hidden));
});
document.addEventListener('keydown', event => { if (event.key === 'Escape' && !menu.hidden) { closeMenu(); cat.focus(); } });
document.addEventListener('click', event => { if (!event.target.closest('.demo')) closeMenu(); });
menu.addEventListener('click', event => { if (event.target.closest('a')) closeMenu(); });
