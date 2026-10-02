"use strict";
const $=id=>document.getElementById(id);
async function api(url,options={}) {
 const response=await fetch(url,options);
 if(response.status===401) throw new Error('Administrator key is incorrect.');
 const data=await response.json();
 if(!response.ok) throw new Error(data.error||'The request could not be completed.');
 return data;
}
function text(tag,value){const element=document.createElement(tag);element.textContent=value;return element;}
function date(value){return new Date(value).toLocaleString('en-PH',{dateStyle:'medium',timeStyle:'short'});}
async function loadEvents(){
 try{
 const events=await api('/api/events');$('events').replaceChildren();
 const selected=$('event').value;const adminSelected=$('admin-event').value;
 for(const id of ['event','admin-event']){$(id).replaceChildren(new Option('Choose an event',''));}
 for(const event of events){
  const card=document.createElement('article');card.append(text('h3',event.title));
  const when=text('time',date(event.startsAt));when.dateTime=event.startsAt;card.append(when);
  card.append(text('p',event.venue));const description=text('p',event.description);description.className='description';card.append(description);
  card.append(text('p',event.availableSeats+' seats available'));
  const button=text('button',event.availableSeats>0?'Register':'Full');button.type='button';button.disabled=event.availableSeats<=0;button.setAttribute('aria-label',(event.availableSeats>0?'Register':'Full')+' - '+event.title);
  button.addEventListener('click',()=>{$('event').value=event.eventId;$('registration').scrollIntoView();$('name').focus();});card.append(button);$('events').append(card);
  const option=new Option(event.title,String(event.eventId));option.disabled=event.availableSeats<=0;$('event').append(option);$('admin-event').append(new Option(event.title,String(event.eventId)));
 }
 $('event').value=selected;$('admin-event').value=adminSelected;$('catalog-status').textContent=events.length?'Choose an event below.':'No upcoming events.';
 }catch(error){$('catalog-status').textContent='Unable to load events. Check the server and reload.';}
}
$('registration-form').addEventListener('submit',async event=>{
 event.preventDefault();const email=$('email').value.trim().toLowerCase();
 if(!/^[^\s@]+@univ\.edu\.ph$/.test(email)){$('registration-status').textContent='Use your @univ.edu.ph student email.';$('email').focus();return;}
 $('register-button').disabled=true;$('registration-status').textContent='Submitting...';
 try{const result=await api('/api/registrations',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({eventId:Number($('event').value),fullName:$('name').value.trim(),email})});$('registration-status').textContent='Registration confirmed. Reference '+result.registrationId+'.';$('registration-form').reset();await loadEvents();}
 catch(error){$('registration-status').textContent=error.message;}
 finally{$('register-button').disabled=false;}
});
$('admin-form').addEventListener('submit',async event=>{
 event.preventDefault();$('attendees').replaceChildren();$('admin-status').textContent='Loading...';
 try{const rows=await api('/api/admin/events/'+Number($('admin-event').value)+'/attendees',{headers:{'X-Admin-Key':$('admin-key').value}});
 for(const row of rows){const tr=document.createElement('tr');for(const value of [row.fullName,row.email,date(row.registeredAt)])tr.append(text('td',value));$('attendees').append(tr);}
 $('admin-status').textContent=rows.length+' attendees found.';}
 catch(error){$('admin-status').textContent=error.message;}
});
loadEvents();
