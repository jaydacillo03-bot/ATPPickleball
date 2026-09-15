// OBS-compatible scoreboard overlay
(function(){
  function qs(name, fallback){
    const params = new URLSearchParams(window.location.search);
    return params.get(name) ?? fallback;
  }

  const teamAEl = document.getElementById('teamA');
  const teamBEl = document.getElementById('teamB');
  const scoreAEl = document.getElementById('scoreA');
  const scoreBEl = document.getElementById('scoreB');

  // Apply initial values from query params
  teamAEl.textContent = qs('teamAName','Team A');
  teamBEl.textContent = qs('teamBName','Team B');
  scoreAEl.textContent = qs('scoreA','0');
  scoreBEl.textContent = qs('scoreB','0');

  // Allow overriding CSS via query params (hex colors)
  const textColor = qs('textColor');
  const accentColor = qs('accentColor');
  if(textColor) document.documentElement.style.setProperty('--text', textColor);
  if(accentColor) document.documentElement.style.setProperty('--accent', accentColor);

  // Update function
  function applyUpdate(obj){
    if(obj.teamAName!=null) teamAEl.textContent = obj.teamAName;
    if(obj.teamBName!=null) teamBEl.textContent = obj.teamBName;
    if(obj.scoreA!=null) scoreAEl.textContent = String(obj.scoreA);
    if(obj.scoreB!=null) scoreBEl.textContent = String(obj.scoreB);
  }

  // Listen for window.postMessage events (useful for local controllers)
  window.addEventListener('message', (ev)=>{
    try{ const d = typeof ev.data === 'string' ? JSON.parse(ev.data) : ev.data; if(d && d.type === 'update') applyUpdate(d); }catch(e){}
  }, false);

  // If ws parameter is provided, connect to WebSocket for updates
  const wsUrl = qs('ws','');
  if(wsUrl){
    try{
      const ws = new WebSocket(wsUrl);
      ws.addEventListener('message', (m)=>{
        try{ const payload = JSON.parse(m.data); applyUpdate(payload); }catch(e){}
      });
      ws.addEventListener('open', ()=>console.log('obs_scoreboard: ws open'));
      ws.addEventListener('close', ()=>console.log('obs_scoreboard: ws closed'));
      ws.addEventListener('error', (err)=>console.warn('obs_scoreboard ws error', err));
    }catch(e){console.warn('obs_scoreboard: ws failed', e)}
  }

  // Expose a small API on window for browser-console updates
  window.OBSScoreboard = {
    update: applyUpdate
  };

})();
