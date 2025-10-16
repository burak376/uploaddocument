import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { CssBaseline, ThemeProvider, createTheme } from '@mui/material'
import { BrowserRouter } from 'react-router-dom'
import App from './App.tsx'
import './index.css'

const queryClient = new QueryClient()
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#0056b3' },
    secondary: { main: '#f57c00' },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </ThemeProvider>
    </QueryClientProvider>
  </StrictMode>,
)
