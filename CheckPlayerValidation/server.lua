
local OnlinePlayers = {}
local SteamPlayersLeft = {}

AddEventHandler('playerDropped', function()
local identifier = GetPlayerIdentifiers(source)[1]
	PerformHttpRequest('http://91.121.73.186/JoinApi/Update.php', function(err, text, headers) end, 'POST', json.encode({steam = identifier, timestamp=os.date('%Y-%m-%d %H:%M:%S')}), { ['Content-Type'] = 'application/json' })
	table.insert(SteamPlayersLeft, GetPlayerIdentifiers(source)[1])
end)

AddEventHandler( "playerConnecting", function(name, setReason )
	local identifier = GetPlayerIdentifiers(source)[1]
	if not isPlayerAuthorised(identifier) then
		setReason("Tu dois te co avec le launcher.")
		print("(" .. identifier .. ") " .. name .. " a été kick ne s'ets pas co avec le launcher")
		CancelEvent()
		return
    end
	for k,v in pairs (SteamPlayersLeft) do
		if v == identifier then
			table.remove(SteamPlayersLeft, k)
		end
	end
end)

function isPlayerAuthorised(identifier)
	local result = MySQL.Sync.fetchScalar("SELECT steam FROM playerjoin WHERE steam = @identifier", {['@identifier'] = identifier})
	if result then
		return true
	end
	return false
end

Citizen.CreateThread(function()
	while true do
	Citizen.Wait(5000) -- waits 5 sec.
	PerformHttpRequest("http://91.121.73.186/JoinApi/Read.php", function(err, LoggedInUsers, headers)
		OnlinePlayers = json.decode(LoggedInUsers)
		local date = os.date('%Y-%m-%d %H:%M:%S', os.time())
		local year, month, day, hours, minutes, seconds = date:match('^(%d%d%d%d)-(%d%d)-(%d%d) (%d%d):(%d%d):(%d%d)$')
		local dt = {year=year, month=month, day=day, hour=hours, min=minutes, sec=seconds}
		if OnlinePlayers.records ~=nil then
			for k,v in pairs (OnlinePlayers.records) do
				for key, value in pairs (SteamPlayersLeft) do
					if v.steam == value then
						local date2 = v.timestamp
						local year2, month2, day2, hours2, minutes2, seconds2 = date2:match('^(%d%d%d%d)-(%d%d)-(%d%d) (%d%d):(%d%d):(%d%d)$')
						local dt2 = {year=year2, month=month2, day=day2, hour=hours2, min=minutes2, sec=seconds2}
						local calculation = os.time(dt) - os.time(dt2)
						if calculation > 150 then
						table.remove(SteamPlayersLeft, key)
							PerformHttpRequest('http://91.121.73.186/JoinApi/Delete.php', function(err, text, headers) end, 'POST', json.encode({steam = v.steam}), { ['Content-Type'] = 'application/json' })
						end
					end
				end
			end
		end
	end, "GET", "", {what = 'this'})
	end
end)

AddEventHandler('onMySQLReady', function()
  MySQL.Async.execute(
	'DELETE FROM playerjoin',
	{}
  )

end)


function dump(o, nb)
  if nb == nil then
    nb = 0
  end
   if type(o) == 'table' then
      local s = ''
      for i = 1, nb + 1, 1 do
        s = s .. "    "
      end
      s = '{\n'
      for k,v in pairs(o) do
         if type(k) ~= 'number' then k = '"'..k..'"' end
          for i = 1, nb, 1 do
            s = s .. "    "
          end
         s = s .. '['..k..'] = ' .. dump(v, nb + 1) .. ',\n'
      end
      for i = 1, nb, 1 do
        s = s .. "    "
      end
      return s .. '}'
   else
      return tostring(o)
   end
end
