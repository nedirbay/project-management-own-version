# 🚀 Quick Start Guide - New Features

## 📦 Installation

### 1. Install Required Packages

```bash
# Install vue-i18n for internationalization
npm install vue-i18n@9

# Install FullCalendar for calendar view
npm install @fullcalendar/vue3 @fullcalendar/core @fullcalendar/daygrid @fullcalendar/timegrid @fullcalendar/interaction @fullcalendar/list
```

### 2. Run the Application

```bash
npm install
npm run dev
```

---

## 🌍 Internationalization (i18n)

### Change Language

1. Go to **Settings** (Ayarlar)
2. Select **Preferences** (Tercihler) tab
3. Choose your language from dropdown:
   - 🇹🇷 Türkçe
   - 🇬🇧 English
4. UI instantly translates!

### Using i18n in Your Code

```vue
<template>
  <!-- In template -->
  <h1>{{ $t('dashboard.title') }}</h1>
  <button>{{ $t('common.save') }}</button>
</template>

<script setup>
// In script
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

ElMessage.success(t('tasks.created'))
</script>
```

### Add New Language

```typescript
// 1. Create new locale file: src/i18n/locales/de.ts
export default {
  common: {
    save: 'Speichern',
    cancel: 'Abbrechen'
  }
}

// 2. Import in src/i18n/index.ts
import de from './locales/de'

// 3. Add to messages
const i18n = createI18n({
  messages: { en, tr, de }
})

// 4. Add to availableLocales
export const availableLocales = [
  { code: 'tr', name: 'Türkçe', flag: '🇹🇷' },
  { code: 'en', name: 'English', flag: '🇬🇧' },
  { code: 'de', name: 'Deutsch', flag: '🇩🇪' }
]
```

---

## 📅 Calendar View

### Access Calendar

1. Go to **Tasks** page
2. Click **Calendar View** button (top right)
3. See your tasks on calendar!

### Calendar Modes

- **Month**: Full month overview
- **Week**: Weekly detailed view
- **Day**: Daily hour-by-hour
- **List**: Chronological list

### Drag to Reschedule

1. Click and hold a task on calendar
2. Drag to new date
3. Drop it
4. Task automatically updates! ✅

### Filter Tasks

Use filters at the top:
- **Workspace**: Select workspace
- **Project**: Filter by project
- **Status**: Multiple status selection
- **Search**: Type to search

### Create Task from Calendar

1. Click on any date
2. Task dialog opens with that date pre-filled
3. Fill details and save

### Color Coding

- 🟢 **Green**: Low priority
- 🔵 **Blue**: Medium priority
- 🟡 **Yellow**: High priority
- 🔴 **Red**: Critical priority
- Border color = Project color
- Strikethrough = Completed tasks

---

## 👤 Profile Management

### Upload Avatar

1. Go to **Profile** page
2. Click on avatar or "Upload Avatar" button
3. Select image (max 5MB)
4. See instant preview
5. Done! ✨

**Validation:**
- Max file size: 5MB
- Accepted: Images only (jpg, png, gif, etc.)

### Change Password

1. Go to **Profile** > **Security** section
2. Enter current password
3. Enter new password
4. See password strength indicator:
   - 🔴 **Weak**: Red bar
   - 🟡 **Medium**: Yellow bar
   - 🟢 **Strong**: Green bar
5. Confirm password
6. Click "Change Password"

**Strong Password Requirements:**
- At least 8 characters
- 1 uppercase letter (A-Z)
- 1 lowercase letter (a-z)
- 1 number (0-9)
- 1 special character (optional, makes it stronger)

### Notification Settings

1. Go to **Profile** > **Notifications**
2. Toggle notifications on/off:
   - Task assigned to you
   - Task completed
   - Project updated
   - Daily report reminder
   - Browser notifications
3. Settings auto-save ✅

---

## ⚙️ Settings Management

### Theme Settings

1. Go to **Settings** > **Appearance**
2. Select theme:
   - ☀️ **Light Theme**
   - 🌙 **Dark Theme**
   - 💻 **System Theme** (auto-detects OS preference)
3. See live preview
4. Theme applies instantly!

### Language & Timezone

1. Go to **Settings** > **Preferences**
2. **Language**: Select Türkçe or English
3. **Timezone**: Choose from 11+ timezones
4. **Date Format**: Select format (DD/MM/YYYY, MM/DD/YYYY, etc.)
5. **Time Format**: 12h or 24h
6. All changes auto-save!

### Available Timezones

- Istanbul (GMT+3)
- London (GMT+0)
- Paris (GMT+1)
- New York (GMT-5)
- Los Angeles (GMT-8)
- Tokyo (GMT+9)
- Sydney (GMT+10)
- ...and more!

---

## 🎯 Quick Tips

### i18n
- ✅ All 260+ UI elements are translated
- ✅ Language preference persists after refresh
- ✅ Add new languages easily
- ✅ Use `$t()` anywhere in templates

### Calendar
- ✅ Drag & drop works smoothly
- ✅ All filters apply to calendar
- ✅ Click task to see details
- ✅ Color-coded by priority
- ✅ Responsive on mobile

### Profile
- ✅ Avatar size: max 5MB
- ✅ Password strength indicator
- ✅ Notifications save automatically
- ✅ Stats show on profile card

### Settings
- ✅ All settings in Pinia store
- ✅ Persisted in LocalStorage
- ✅ Changes apply instantly
- ✅ Reset to defaults available

---

## 📱 Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Switch to list view | `L` |
| Switch to calendar | `C` |
| Create new task | `N` |
| Search tasks | `/` |
| Open settings | `S` |
| Toggle theme | `T` |

*(Coming soon)*

---

## 🐛 Troubleshooting

### Calendar not showing?
- Make sure you installed FullCalendar packages
- Check browser console for errors
- Try refreshing the page

### Language not changing?
- Clear browser cache
- Check LocalStorage for `locale` key
- Make sure vue-i18n@9 is installed

### Avatar not uploading?
- Check file size (max 5MB)
- Use image files only
- Check browser permissions

### Settings not saving?
- Check browser LocalStorage support
- Try different browser
- Clear site data and retry

---

## 🚀 Advanced Usage

### Custom i18n Messages

```typescript
// Add custom namespace
export default {
  myFeature: {
    title: 'My Feature',
    button: 'Click Me'
  }
}

// Use in template
{{ $t('myFeature.title') }}
```

### Custom Calendar Events

```typescript
const customEvent = {
  id: 'custom-1',
  title: 'My Event',
  start: '2024-01-15',
  backgroundColor: '#ff0000',
  extendedProps: {
    custom: 'data'
  }
}
```

### Store Integration

```typescript
import { useSettingsStore } from '@/stores/settings'

const settingsStore = useSettingsStore()

// Get current settings
console.log(settingsStore.settings)

// Update theme
settingsStore.updateTheme('dark')

// Update language
settingsStore.updateLanguage('en')
```

---

## 📚 File Structure

```
src/
├── i18n/
│   ├── index.ts              # i18n config
│   └── locales/
│       ├── tr.ts             # Turkish translations
│       └── en.ts             # English translations
├── stores/
│   └── settings.ts           # Settings Pinia store
├── views/
│   ├── TasksView.vue         # Calendar + List view
│   ├── ProfileView.vue       # Enhanced profile
│   └── SettingsView.vue      # Settings management
└── main.ts                   # App initialization
```

---

## 🎓 Learn More

### Documentation
- [Vue i18n](https://vue-i18n.intlify.dev/)
- [FullCalendar Vue3](https://fullcalendar.io/docs/vue)
- [Pinia](https://pinia.vuejs.org/)
- [Element Plus](https://element-plus.org/)

### API References
- [FileReader API](https://developer.mozilla.org/en-US/docs/Web/API/FileReader)
- [LocalStorage API](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage)

---

## ✅ Checklist

Before you start:
- [ ] Install vue-i18n@9
- [ ] Install FullCalendar packages
- [ ] Run `npm install`
- [ ] Start dev server
- [ ] Open http://localhost:5173
- [ ] Try changing language
- [ ] Try calendar view
- [ ] Upload avatar
- [ ] Change theme

---

## 💡 Pro Tips

1. **Use Calendar for Planning**: Drag tasks to schedule your week
2. **Customize Notifications**: Turn off distractions, keep important ones
3. **Try Both Themes**: Find what's comfortable for your eyes
4. **Set Your Timezone**: See accurate times for all tasks
5. **Upload Avatar**: Personalize your profile
6. **Strong Password**: Use the strength indicator guide

---

## 🎉 You're Ready!

All features are now at your fingertips:
- 🌍 Multi-language support
- 📅 Powerful calendar view
- 👤 Enhanced profile management
- ⚙️ Comprehensive settings

**Enjoy your enhanced Project Manager experience!** 🚀

---

**Need Help?**
- Check FEATURES.md for detailed documentation
- See README.md for full project info
- Open GitHub issues for bugs
- Contact: support@projectmanager.com

**Version:** 2.0.0  
**Last Updated:** 2024