import { Routes } from '@angular/router';
import { AlertsConfigComponent } from './pages/alerts-config/alerts-config.component';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './pages/Auth/login/login.component';
import { HomeComponent } from './pages/home/home.component';
import { VoucherProvidersComponent } from './pages/voucher-providers/voucher-providers.component';

export const routes: Routes = [
    {path: '', component: LoginComponent},
    {path: 'home', component: HomeComponent},
    {path: 'admin/voucherproviders', component: VoucherProvidersComponent, canActivate: [authGuard]},
    {path: 'admin/alertsconfig', component: AlertsConfigComponent, canActivate: [authGuard]},
    {path: 'auth/login', component: LoginComponent},
];
